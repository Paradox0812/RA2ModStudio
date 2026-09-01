extern alias Ra2Application;

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using RA2IniEditor.IDE.AssetAuthoring;
using RA2IniEditor.IDE.ViewModels.AssetAuthoring;
using Ra2Rgba32 = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2Rgba32;
using Ra2VoxelGeometryRegionMask = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelGeometryRegionMask;
using Ra2VoxelFeatureProtectionMask = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelFeatureProtectionMask;
using Ra2VoxelSemanticPartition = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticPartition;
using Ra2VoxelSceneSnapshot = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSceneSnapshot;
using Ra2VoxelSemanticEvidencePackage = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticEvidencePackage;
using Ra2VoxelSemanticEffectiveAssignment = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticEffectiveAssignment;
using Ra2VoxelCoordinate = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelCoordinate;
using Ra2VoxelSemanticMaskComposition = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticMaskComposition;
using Ra2VoxelSemanticMaskEditor = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticMaskEditor;
using Ra2VoxelFormZoneProjection = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelFormZoneProjection;
using Ra2VoxelBoundaryIntentProjection = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelBoundaryIntentProjection;
using Ra2VoxelFeatureScaleProjection = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelFeatureScaleProjection;
using Ra2VoxelColourQualityReport = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelColourQualityReport;

namespace RA2IniEditor.IDE.Views.AssetAuthoring;

internal sealed record Ra2VoxelSemanticCellHit(string RegionId, Ra2VoxelCoordinate Coordinate);

internal enum Ra2VoxelSemanticHitFailureKind
{
    NoScene = 0,
    SemanticUnavailable,
    Background,
    SceneMismatch,
    RegionUnavailable
}

internal sealed record Ra2VoxelSemanticHitFailure(
    Ra2VoxelSemanticHitFailureKind Kind,
    string Message);

internal sealed class Ra2VoxelSemanticStrokeStartingEventArgs(Ra2VoxelSemanticCellHit hit) : EventArgs
{
    internal Ra2VoxelSemanticCellHit Hit { get; } = hit;
    internal bool IsAccepted { get; set; }
}

internal sealed class Ra2VoxelSemanticStrokeProgressEventArgs(int seedCount) : EventArgs
{
    internal int SeedCount { get; } = seedCount;
}

internal sealed class Ra2VoxelSemanticStrokeCompletedEventArgs(IReadOnlyList<Ra2VoxelCoordinate> seeds) : EventArgs
{
    internal IReadOnlyList<Ra2VoxelCoordinate> Seeds { get; } = Array.AsReadOnly(seeds.ToArray());
}

internal sealed class Ra2VoxelSemanticStrokeCanceledEventArgs(string message) : EventArgs
{
    internal string Message { get; } = message;
}

internal partial class Ra2VoxelViewport3D : UserControl, IDisposable
{
    private const double MinimumPitch = -1.35d;
    private const double MaximumPitch = 1.35d;
    internal const double StrokeSampleSpacing = 4d;
    internal const int MaximumStrokeSamplesPerMove = 4096;
    internal const int MaximumStrokeSeedCount = Ra2VoxelSemanticMaskEditor.MaximumStrokeSeedCount;
    private CancellationTokenSource? _sceneCancellation;
    private long _sceneGeneration;
    private Rect3D _bounds = Rect3D.Empty;
    private Point3D _target;
    private double _distance = 10d;
    private double _yaw = -0.78d;
    private double _pitch = 0.48d;
    private string? _cameraGroupKey;
    private Ra2VoxelViewportCameraState? _savedCameraState;
    private Ra2VoxelViewportCameraState? _gameScaleRestoreState;
    private bool _hasUserInteraction;
    private Point _lastPointer;
    private DragMode _dragMode;
    private MouseButton? _dragButton;
    private Ra2VoxelSceneSnapshot? _snapshot;
    private Ra2VoxelSemanticEvidencePackage? _semanticEvidence;
    private Ra2VoxelViewportSceneHitMap _hitMap = Ra2VoxelViewportSceneHitMap.Empty;
    private Dictionary<Ra2VoxelCoordinate, int> _cellIndices = [];
    private Ra2VoxelSemanticEditMode _semanticEditMode;
    private SemanticStrokeState? _semanticStroke;
    private long _lastStrokePreviewTimestamp;
    private bool _disposed;

    public Ra2VoxelViewport3D()
    {
        InitializeComponent();
        UpdateCamera();
    }

    internal event EventHandler<string>? SceneBuildFailed;
    internal event EventHandler<Ra2VoxelSemanticCellHit>? SemanticCellSelected;
    internal event EventHandler<Ra2VoxelSemanticHitFailure>? SemanticCellHitFailed;
    internal event EventHandler<Ra2VoxelSemanticStrokeStartingEventArgs>? SemanticStrokeStarting;
    internal event EventHandler<Ra2VoxelSemanticStrokeProgressEventArgs>? SemanticStrokeProgress;
    internal event EventHandler<Ra2VoxelSemanticStrokeCompletedEventArgs>? SemanticStrokeCompleted;
    internal event EventHandler<Ra2VoxelSemanticStrokeCanceledEventArgs>? SemanticStrokeCanceled;

    internal async Task SetSceneAsync(
        Ra2VoxelSceneSnapshot snapshot,
        Ra2VoxelGeometryRegionMask? geometryMask,
        Ra2VoxelViewportColourMode colourMode,
        Ra2VoxelSceneSnapshot? comparisonSnapshot = null,
        Ra2VoxelFeatureProtectionMask? protectionMask = null,
        Ra2VoxelSemanticPartition? semanticPartition = null,
        Ra2VoxelSemanticEvidencePackage? semanticEvidence = null,
        IReadOnlyList<Ra2VoxelSemanticEffectiveAssignment>? semanticAssignments = null,
        Ra2VoxelSemanticMaskComposition? semanticComposition = null,
        Ra2VoxelSemanticReviewDimension semanticReviewDimension = Ra2VoxelSemanticReviewDimension.Material,
        Ra2VoxelFormZoneProjection? formZones = null,
        Ra2VoxelBoundaryIntentProjection? boundaryIntents = null,
        Ra2VoxelFeatureScaleProjection? featureScale = null,
        Ra2VoxelSceneSnapshot? riskCandidate = null,
        Ra2VoxelSemanticMaskComposition? riskComposition = null,
        Ra2VoxelColourQualityReport? quality = null,
        string? cameraGroupKey = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(snapshot);
        CancelSemanticStroke("3D 场景正在更新，未完成的笔划已取消。");
        CancelPendingScene();
        _sceneCancellation = new CancellationTokenSource();
        CancellationToken cancellationToken = _sceneCancellation.Token;
        long generation = Interlocked.Increment(ref _sceneGeneration);
        ShowStatus("正在生成 3D 外露面…");

        Ra2VoxelViewportSceneBuildResult result = await Task.Run(
            () => Ra2VoxelViewportSceneBuilder.Build(snapshot, geometryMask, colourMode,
                comparisonSnapshot, protectionMask, semanticPartition,
                semanticEvidence, semanticAssignments, semanticComposition,
                semanticReviewDimension,
                formZones, boundaryIntents, featureScale, riskCandidate, riskComposition, quality,
                cancellationToken: cancellationToken),
            CancellationToken.None);
        if (_disposed || cancellationToken.IsCancellationRequested || generation != Interlocked.Read(ref _sceneGeneration))
            return;

        HideStatus();
        if (!result.IsSuccess || result.Model is null)
        {
            if (result.FailureKind != Ra2VoxelViewportSceneFailureKind.Cancelled)
                SceneBuildFailed?.Invoke(this, result.Message);
            return;
        }

        Ra2VoxelViewportCameraState? cameraState =
            string.Equals(_cameraGroupKey, cameraGroupKey, StringComparison.Ordinal)
                ? _savedCameraState
                : null;
        SceneVisual.Content = result.Model;
        _snapshot = snapshot;
        _semanticEvidence = semanticEvidence;
        _hitMap = result.HitMap;
        _cellIndices = snapshot.Cells.Select((cell, index) => (cell.Coordinate, index))
            .ToDictionary(value => value.Coordinate, value => value.index);
        _bounds = result.Bounds;
        _cameraGroupKey = cameraGroupKey;
        if (cameraState is not { } savedCameraState || !RestoreCamera(savedCameraState))
            ResetCamera();
    }

    internal void ClearScene()
    {
        CancelSemanticStroke("3D 场景已清除，未完成的笔划已取消。");
        EndDrag();
        CancelPendingScene();
        SceneVisual.Content = null;
        _snapshot = null;
        _semanticEvidence = null;
        _hitMap = Ra2VoxelViewportSceneHitMap.Empty;
        _cellIndices.Clear();
        _bounds = Rect3D.Empty;
        ShowStatus("尚未载入可显示的 3D 模型。");
    }

    internal void CancelSceneBuild()
    {
        CancelSemanticStroke("3D 场景构建已取消，未完成的笔划已取消。");
        CancelPendingScene();
    }

    internal void SetSemanticEditMode(Ra2VoxelSemanticEditMode mode)
    {
        if (!Enum.IsDefined(mode) || _semanticEditMode == mode)
            return;
        CancelSemanticStroke("语义编辑模式已切换，未完成的笔划已取消。");
        _semanticEditMode = mode;
    }

    internal void ResetCamera()
    {
        if (_bounds.IsEmpty)
            return;
        _target = new(
            _bounds.X + (_bounds.SizeX / 2d),
            _bounds.Y + (_bounds.SizeY / 2d),
            _bounds.Z + (_bounds.SizeZ / 2d));
        double diagonal = Math.Sqrt((_bounds.SizeX * _bounds.SizeX) +
            (_bounds.SizeY * _bounds.SizeY) + (_bounds.SizeZ * _bounds.SizeZ));
        _distance = Math.Max(4d, diagonal * 1.7d);
        _yaw = -0.78d;
        _pitch = 0.48d;
        _hasUserInteraction = false;
        UpdateCamera();
    }

    internal bool EnterGameScaleReview()
    {
        if (_bounds.IsEmpty || _gameScaleRestoreState is not null)
            return false;
        if (!Ra2VoxelViewportCameraState.TryCapture(
                _bounds, _target, _distance, _yaw, _pitch, _hasUserInteraction, out Ra2VoxelViewportCameraState state))
            return false;
        _gameScaleRestoreState = state;
        double diagonal = Math.Sqrt((_bounds.SizeX * _bounds.SizeX) +
            (_bounds.SizeY * _bounds.SizeY) + (_bounds.SizeZ * _bounds.SizeZ));
        _distance = Math.Max(8d, diagonal * 4.8d);
        _hasUserInteraction = false;
        UpdateCamera();
        return true;
    }

    internal bool ExitGameScaleReview()
    {
        if (_gameScaleRestoreState is not { } state)
            return false;
        _gameScaleRestoreState = null;
        return RestoreCamera(state);
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        CancelSemanticStroke("3D 视图已关闭，未完成的笔划已取消。");
        EndDrag();
        CancelPendingScene();
        SceneVisual.Content = null;
        StrokePreviewVisual.Content = null;
        _hitMap = Ra2VoxelViewportSceneHitMap.Empty;
        _cellIndices.Clear();
    }

    private void InputSurface_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Focus();
        Point point = e.GetPosition(Viewport);
        if (!TryResolveSemanticHit(point, out Ra2VoxelSemanticCellHit? hit, out Ra2VoxelSemanticHitFailure? failure))
        {
            if (failure is not null)
                FailHit(failure.Kind, failure.Message);
            e.Handled = true;
            return;
        }

        if (_semanticEditMode == Ra2VoxelSemanticEditMode.Browse)
        {
            SemanticCellSelected?.Invoke(this, hit!);
            e.Handled = true;
            return;
        }

        Ra2VoxelSemanticStrokeStartingEventArgs starting = new(hit!);
        SemanticStrokeStarting?.Invoke(this, starting);
        if (!starting.IsAccepted)
        {
            e.Handled = true;
            return;
        }

        _semanticStroke = new(Interlocked.Read(ref _sceneGeneration), _semanticEditMode, point, hit!.Coordinate);
        _lastStrokePreviewTimestamp = 0;
        if (!InputSurface.CaptureMouse())
        {
            CancelSemanticStroke("无法捕获鼠标，笔划未开始。", releaseCapture: false);
            e.Handled = true;
            return;
        }
        UpdateStrokePreview(force: true);
        SemanticStrokeProgress?.Invoke(this, new(_semanticStroke.Seeds.Count));
        e.Handled = true;
    }

    private void InputSurface_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        CancelSemanticStroke("已取消未完成的笔划并切换到相机操作。");
        BeginDrag(e, Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? DragMode.Pan : DragMode.Orbit);
    }

    private void InputSurface_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Middle)
        {
            CancelSemanticStroke("已取消未完成的笔划并切换到相机操作。");
            BeginDrag(e, DragMode.Pan);
        }
    }

    private void BeginDrag(MouseButtonEventArgs e, DragMode dragMode)
    {
        _dragMode = dragMode;
        _dragButton = e.ChangedButton;
        _lastPointer = e.GetPosition(Viewport);
        InputSurface.CaptureMouse();
        Focus();
        e.Handled = true;
    }

    private void InputSurface_OnMouseButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && _semanticStroke is not null)
        {
            CompleteSemanticStroke();
            e.Handled = true;
            return;
        }
        if (_dragMode == DragMode.None || e.ChangedButton != _dragButton)
            return;
        EndDrag();
        e.Handled = true;
    }

    private void InputSurface_OnLostMouseCapture(object sender, MouseEventArgs e)
    {
        CancelSemanticStroke("鼠标捕获已丢失，未完成的笔划已取消。", releaseCapture: false);
        EndDrag(releaseCapture: false);
    }

    private void EndDrag(bool releaseCapture = true)
    {
        _dragMode = DragMode.None;
        _dragButton = null;
        if (releaseCapture && InputSurface.IsMouseCaptured)
            InputSurface.ReleaseMouseCapture();
    }

    private bool TryResolveSemanticHit(
        Point point,
        out Ra2VoxelSemanticCellHit? semanticHit,
        out Ra2VoxelSemanticHitFailure? failure)
    {
        semanticHit = null;
        failure = null;
        if (_snapshot is null)
        {
            failure = new(Ra2VoxelSemanticHitFailureKind.NoScene, "当前没有可点击的 3D 模型。");
            return false;
        }
        if (_semanticEvidence is null)
        {
            failure = new(Ra2VoxelSemanticHitFailureKind.SemanticUnavailable, "当前不是语义预览，请先选择浏览、画笔或擦除。");
            return false;
        }

        Ra2VoxelCoordinate coordinate = default;
        int selectedIndex = -1;
        bool sawMesh = false;
        VisualTreeHelper.HitTest(
            Viewport,
            filterCallback: null,
            resultCallback: result =>
            {
                if (result is not RayMeshGeometry3DHitTestResult meshHit)
                    return HitTestResultBehavior.Continue;
                sawMesh = true;
                if (!_hitMap.TryResolve(meshHit.ModelHit, meshHit.VertexIndex1, meshHit.VertexIndex2, meshHit.VertexIndex3,
                        out Ra2VoxelCoordinate candidate) || !_cellIndices.TryGetValue(candidate, out int candidateIndex))
                    return HitTestResultBehavior.Continue;
                coordinate = candidate;
                selectedIndex = candidateIndex;
                return HitTestResultBehavior.Stop;
            },
            hitTestParameters: new PointHitTestParameters(point));
        if (selectedIndex < 0)
        {
            failure = sawMesh
                ? new(Ra2VoxelSemanticHitFailureKind.SceneMismatch, "当前场景命中数据已过期，请重新进入语义预览。")
                : new(Ra2VoxelSemanticHitFailureKind.Background, "未命中模型表面。");
            return false;
        }
        var region = _semanticEvidence.Regions.FirstOrDefault(value =>
            selectedIndex < value.Selected.Count && value.Selected[selectedIndex] != 0);
        if (region is null)
        {
            failure = new(Ra2VoxelSemanticHitFailureKind.RegionUnavailable, "命中体素尚未归入当前语义区域，请重新准备区域。");
            return false;
        }
        semanticHit = new(region.RegionId, coordinate);
        return true;
    }

    private bool FailHit(Ra2VoxelSemanticHitFailureKind kind, string message)
    {
        SemanticCellHitFailed?.Invoke(this, new(kind, message));
        return false;
    }

    private void InputSurface_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (_semanticStroke is not null)
        {
            ContinueSemanticStroke(e.GetPosition(Viewport));
            e.Handled = true;
            return;
        }
        if (_dragMode == DragMode.None || !InputSurface.IsMouseCaptured)
            return;
        Point current = e.GetPosition(Viewport);
        Vector delta = current - _lastPointer;
        _lastPointer = current;
        if (_dragMode == DragMode.Orbit)
        {
            _yaw -= delta.X * 0.009d;
            _pitch = Math.Clamp(_pitch + (delta.Y * 0.009d), MinimumPitch, MaximumPitch);
        }
        else
        {
            Vector3D look = Camera.LookDirection;
            look.Normalize();
            Vector3D right = Vector3D.CrossProduct(look, Camera.UpDirection);
            right.Normalize();
            Vector3D up = Vector3D.CrossProduct(right, look);
            up.Normalize();
            double scale = _distance * 0.0018d;
            _target += (-delta.X * scale * right) + (delta.Y * scale * up);
        }
        _hasUserInteraction = true;
        UpdateCamera();
        e.Handled = true;
    }

    internal static IReadOnlyList<Point> InterpolateStrokePoints(Point from, Point to)
    {
        Vector delta = to - from;
        double distance = delta.Length;
        int steps = Math.Max(1, (int)Math.Ceiling(distance / StrokeSampleSpacing));
        if (steps > MaximumStrokeSamplesPerMove)
            throw new InvalidOperationException($"单次鼠标移动需要超过 {MaximumStrokeSamplesPerMove:N0} 个采样点。");
        Point[] points = new Point[steps];
        for (int index = 1; index <= steps; index++)
            points[index - 1] = from + (delta * ((double)index / steps));
        return Array.AsReadOnly(points);
    }

    private void ContinueSemanticStroke(Point current)
    {
        SemanticStrokeState stroke = _semanticStroke!;
        if (!InputSurface.IsMouseCaptured || Mouse.LeftButton != MouseButtonState.Pressed ||
            stroke.SceneGeneration != Interlocked.Read(ref _sceneGeneration))
        {
            CancelSemanticStroke("笔划上下文已变化，未完成的笔划已取消。");
            return;
        }

        IReadOnlyList<Point> points;
        try
        {
            points = InterpolateStrokePoints(stroke.LastPoint, current);
        }
        catch (InvalidOperationException exception)
        {
            CancelSemanticStroke(exception.Message);
            return;
        }
        stroke.LastPoint = current;
        bool changed = false;
        foreach (Point point in points)
        {
            if (!TryResolveSemanticHit(point, out Ra2VoxelSemanticCellHit? hit, out Ra2VoxelSemanticHitFailure? failure))
            {
                if (failure?.Kind is Ra2VoxelSemanticHitFailureKind.Background)
                    continue;
                CancelSemanticStroke(failure?.Message ?? "笔划命中失败。");
                return;
            }
            if (stroke.SeedSet.Contains(hit!.Coordinate))
                continue;
            if (stroke.Seeds.Count >= MaximumStrokeSeedCount)
            {
                CancelSemanticStroke($"单条笔划最多包含 {MaximumStrokeSeedCount:N0} 个表面采样点。");
                return;
            }
            stroke.SeedSet.Add(hit.Coordinate);
            stroke.Seeds.Add(hit.Coordinate);
            changed = true;
        }
        if (!changed)
            return;
        SemanticStrokeProgress?.Invoke(this, new(stroke.Seeds.Count));
        UpdateStrokePreview(force: false);
    }

    private void CompleteSemanticStroke()
    {
        SemanticStrokeState stroke = _semanticStroke!;
        _semanticStroke = null;
        ClearStrokePreview();
        if (InputSurface.IsMouseCaptured)
            InputSurface.ReleaseMouseCapture();
        SemanticStrokeCompleted?.Invoke(this, new(stroke.Seeds));
    }

    internal void CancelSemanticStroke(string message) => CancelSemanticStroke(message, releaseCapture: true);

    private void CancelSemanticStroke(string message, bool releaseCapture)
    {
        if (_semanticStroke is null)
            return;
        _semanticStroke = null;
        ClearStrokePreview();
        if (releaseCapture && InputSurface.IsMouseCaptured)
            InputSurface.ReleaseMouseCapture();
        SemanticStrokeCanceled?.Invoke(this, new(message));
    }

    private void UpdateStrokePreview(bool force)
    {
        if (_semanticStroke is null || _snapshot is null)
            return;
        long now = Stopwatch.GetTimestamp();
        if (!force && _lastStrokePreviewTimestamp != 0 &&
            Stopwatch.GetElapsedTime(_lastStrokePreviewTimestamp, now) < TimeSpan.FromMilliseconds(33))
            return;
        _lastStrokePreviewTimestamp = now;
        Ra2Rgba32 colour = _semanticStroke.Mode == Ra2VoxelSemanticEditMode.Erase
            ? new(255, 59, 48, 210)
            : new(255, 212, 0, 210);
        StrokePreviewVisual.Content = Ra2VoxelViewportSceneBuilder.BuildCoordinateOverlay(
            _snapshot,
            _semanticStroke.Seeds,
            colour);
    }

    private void ClearStrokePreview()
    {
        StrokePreviewVisual.Content = null;
        _lastStrokePreviewTimestamp = 0;
    }

    private void InputSurface_OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (_bounds.IsEmpty)
            return;
        double factor = Math.Pow(0.86d, e.Delta / 120d);
        double diagonal = Math.Max(1d, Math.Sqrt((_bounds.SizeX * _bounds.SizeX) +
            (_bounds.SizeY * _bounds.SizeY) + (_bounds.SizeZ * _bounds.SizeZ)));
        _distance = Math.Clamp(_distance * factor, diagonal * 0.25d, diagonal * 12d);
        _hasUserInteraction = true;
        UpdateCamera();
        e.Handled = true;
    }

    private void UpdateCamera()
    {
        double horizontal = Math.Cos(_pitch) * _distance;
        Vector3D offset = new(
            Math.Cos(_yaw) * horizontal,
            Math.Sin(_yaw) * horizontal,
            Math.Sin(_pitch) * _distance);
        Camera.Position = _target + offset;
        Camera.LookDirection = _target - Camera.Position;
        Camera.UpDirection = new(0d, 0d, 1d);
        Camera.NearPlaneDistance = Math.Max(0.01d, _distance / 1000d);
        Camera.FarPlaneDistance = Math.Max(100d, _distance * 50d);
        if (Ra2VoxelViewportCameraState.TryCapture(
            _bounds,
            _target,
            _distance,
            _yaw,
            _pitch,
            _hasUserInteraction,
            out Ra2VoxelViewportCameraState state))
        {
            _savedCameraState = state;
        }
    }

    private bool RestoreCamera(Ra2VoxelViewportCameraState state)
    {
        if (!state.TryRestore(_bounds, out _target, out _distance, out _yaw, out _pitch))
            return false;
        _pitch = Math.Clamp(_pitch, MinimumPitch, MaximumPitch);
        _hasUserInteraction = state.HasUserInteraction;
        UpdateCamera();
        return true;
    }

    private void ShowStatus(string text)
    {
        StatusText.Text = text;
        StatusOverlay.Visibility = Visibility.Visible;
    }

    private void HideStatus() => StatusOverlay.Visibility = Visibility.Collapsed;

    private void CancelPendingScene()
    {
        Interlocked.Increment(ref _sceneGeneration);
        _sceneCancellation?.Cancel();
        _sceneCancellation?.Dispose();
        _sceneCancellation = null;
    }

    private enum DragMode
    {
        None = 0,
        Orbit,
        Pan
    }

    private sealed class SemanticStrokeState(
        long sceneGeneration,
        Ra2VoxelSemanticEditMode mode,
        Point lastPoint,
        Ra2VoxelCoordinate firstSeed)
    {
        internal long SceneGeneration { get; } = sceneGeneration;
        internal Ra2VoxelSemanticEditMode Mode { get; } = mode;
        internal Point LastPoint { get; set; } = lastPoint;
        internal List<Ra2VoxelCoordinate> Seeds { get; } = [firstSeed];
        internal HashSet<Ra2VoxelCoordinate> SeedSet { get; } = [firstSeed];
    }
}
