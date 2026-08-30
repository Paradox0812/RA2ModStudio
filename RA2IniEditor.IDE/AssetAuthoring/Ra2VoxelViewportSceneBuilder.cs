extern alias Ra2Application;

using System.Windows.Media;
using System.Windows.Media.Media3D;
using Ra2Rgba32 = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2Rgba32;
using Ra2VoxelCoordinate = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelCoordinate;
using Ra2VoxelFaceDirection = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelFaceDirection;
using Ra2VoxelGeometryRegionMask = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelGeometryRegionMask;
using Ra2VoxelFeatureProtectionMask = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelFeatureProtectionMask;
using Ra2VoxelSemanticPartition = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticPartition;
using Ra2VoxelSymmetryDisposition = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSymmetryDisposition;
using Ra2VoxelCell = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelCell;
using Ra2VoxelSceneSnapshot = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSceneSnapshot;
using Ra2VoxelSurfaceFace = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSurfaceFace;
using Ra2VoxelSurfaceProjectionFailureKind = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSurfaceProjectionFailureKind;
using Ra2VoxelSurfaceProjector = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSurfaceProjector;
using Ra2VoxelColourReviewPackageBuilder = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelColourReviewPackageBuilder;
using Ra2VoxelSemanticEvidencePackage = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticEvidencePackage;
using Ra2VoxelSemanticEffectiveAssignment = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticEffectiveAssignment;
using Ra2VoxelSemanticMaterialRole = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticMaterialRole;
using Ra2VoxelSemanticPartRole = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticPartRole;
using Ra2VoxelSemanticMaskComposition = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticMaskComposition;

namespace RA2IniEditor.IDE.AssetAuthoring;

internal enum Ra2VoxelViewportColourMode
{
    Palette = 0,
    GeometryRegion,
    Difference,
    SemanticStructure,
    SemanticMask
}

internal enum Ra2VoxelSemanticReviewDimension
{
    Part = 0,
    Material
}

internal sealed record Ra2VoxelSemanticLegendItem(string Display, string ColourHex);

internal static class Ra2VoxelSemanticReviewPalette
{
    internal static IReadOnlyList<Ra2VoxelSemanticLegendItem> PartLegend { get; } = Array.AsReadOnly(
    new Ra2VoxelSemanticLegendItem[]
    {
        new("车体", "#4477AA"),
        new("炮塔", "#AA3377"),
        new("炮管", "#EE7733"),
        new("车轮", "#228833"),
        new("履带", "#CCBB44"),
        new("天线", "#33AADD"),
        new("附加部件", "#EE6677"),
        new("未分类", "#8A8F98")
    });

    internal static IReadOnlyList<Ra2VoxelSemanticLegendItem> MaterialLegend { get; } = Array.AsReadOnly(
    new Ra2VoxelSemanticLegendItem[]
    {
        new("涂装面", "#5B9E52"),
        new("玻璃", "#2DA8D2"),
        new("橡胶", "#2E3137"),
        new("裸金属", "#AAB2B8"),
        new("灯光", "#F6D44B"),
        new("暗部", "#241C2B"),
        new("强调", "#E0683E"),
        new("未分类", "#945FD2")
    });

    internal static Ra2Rgba32 PartColour(Ra2VoxelSemanticPartRole role) => role switch
    {
        Ra2VoxelSemanticPartRole.BodyShell => new(68, 119, 170),
        Ra2VoxelSemanticPartRole.Turret => new(170, 51, 119),
        Ra2VoxelSemanticPartRole.Barrel => new(238, 119, 51),
        Ra2VoxelSemanticPartRole.Wheel => new(34, 136, 51),
        Ra2VoxelSemanticPartRole.Track => new(204, 187, 68),
        Ra2VoxelSemanticPartRole.Antenna => new(51, 170, 221),
        Ra2VoxelSemanticPartRole.Attachment => new(238, 102, 119),
        _ => new(138, 143, 152)
    };

    internal static Ra2Rgba32 MaterialColour(Ra2VoxelSemanticMaterialRole role) => role switch
    {
        Ra2VoxelSemanticMaterialRole.PaintedSurface => new(91, 158, 82),
        Ra2VoxelSemanticMaterialRole.Glass => new(45, 168, 210),
        Ra2VoxelSemanticMaterialRole.Rubber => new(46, 49, 55),
        Ra2VoxelSemanticMaterialRole.BareMetal => new(170, 178, 184),
        Ra2VoxelSemanticMaterialRole.Light => new(246, 212, 75),
        Ra2VoxelSemanticMaterialRole.DarkOpening => new(36, 28, 43),
        Ra2VoxelSemanticMaterialRole.Accent => new(224, 104, 62),
        _ => new(148, 95, 210)
    };
}

internal enum Ra2VoxelViewportSceneFailureKind
{
    None = 0,
    InvalidRegionMask,
    ResourceLimitExceeded,
    Cancelled,
    AnalysisFailed
}

internal sealed record Ra2VoxelViewportSceneBuildResult(
    Ra2VoxelViewportSceneFailureKind FailureKind,
    string Message,
    Model3DGroup? Model,
    Ra2VoxelViewportSceneHitMap HitMap,
    Rect3D Bounds,
    int FaceCount,
    int MaterialCount)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelViewportSceneFailureKind.None && Model is not null &&
        HitMap.FaceCount == FaceCount;
}

/// <summary>
/// Scene-lifetime presentation metadata that resolves one rendered quad back to its canonical voxel.
/// It is intentionally IDE-internal and never participates in snapshot or asset serialization.
/// </summary>
internal sealed class Ra2VoxelViewportSceneHitMap
{
    private readonly Dictionary<GeometryModel3D, Ra2VoxelCoordinate[]> _facesByModel;

    internal static Ra2VoxelViewportSceneHitMap Empty { get; } = new([]);

    internal Ra2VoxelViewportSceneHitMap(
        IEnumerable<KeyValuePair<GeometryModel3D, IReadOnlyList<Ra2VoxelCoordinate>>> facesByModel)
    {
        ArgumentNullException.ThrowIfNull(facesByModel);
        _facesByModel = new(ReferenceEqualityComparer.Instance);
        foreach (KeyValuePair<GeometryModel3D, IReadOnlyList<Ra2VoxelCoordinate>> item in facesByModel)
        {
            ArgumentNullException.ThrowIfNull(item.Key);
            ArgumentNullException.ThrowIfNull(item.Value);
            if (!_facesByModel.TryAdd(item.Key, item.Value.ToArray()))
                throw new ArgumentException("A viewport model may only own one face map.", nameof(facesByModel));
        }
        FaceCount = _facesByModel.Values.Sum(value => value.Length);
    }

    internal int FaceCount { get; }

    internal bool TryResolve(
        Model3D? modelHit,
        int vertexIndex1,
        int vertexIndex2,
        int vertexIndex3,
        out Ra2VoxelCoordinate coordinate)
    {
        coordinate = default;
        if (modelHit is not GeometryModel3D geometryModel ||
            vertexIndex1 < 0 || vertexIndex2 < 0 || vertexIndex3 < 0 ||
            !_facesByModel.TryGetValue(geometryModel, out Ra2VoxelCoordinate[]? faces))
        {
            return false;
        }

        int faceIndex = vertexIndex1 / 4;
        if ((vertexIndex2 / 4) != faceIndex || (vertexIndex3 / 4) != faceIndex ||
            faceIndex < 0 || faceIndex >= faces.Length)
        {
            return false;
        }

        coordinate = faces[faceIndex];
        return true;
    }
}

/// <summary>
/// Converts the canonical, format-neutral surface projection into frozen WPF presentation geometry.
/// The returned model is derived session state and never owns or mutates voxel content.
/// </summary>
internal static class Ra2VoxelViewportSceneBuilder
{
    internal const int DefaultMaximumFaceCount = 180_000;

    internal static Ra2VoxelViewportSceneBuildResult Build(
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
        int maximumFaceCount = DefaultMaximumFaceCount,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (colourMode == Ra2VoxelViewportColourMode.GeometryRegion &&
            (geometryMask is null || geometryMask.CellCount != snapshot.OccupancyCount ||
             !string.Equals(geometryMask.SourceSnapshotHash, snapshot.CanonicalHash, StringComparison.Ordinal)))
        {
            return Failure(Ra2VoxelViewportSceneFailureKind.InvalidRegionMask, "几何区域蒙版与当前模型不匹配。");
        }
        if (colourMode == Ra2VoxelViewportColourMode.Difference &&
            (comparisonSnapshot is null || !HasSameGrid(snapshot, comparisonSnapshot)))
        {
            return Failure(Ra2VoxelViewportSceneFailureKind.InvalidRegionMask, "差异参考与当前模型不匹配。");
        }
        if (colourMode == Ra2VoxelViewportColourMode.SemanticStructure &&
            (semanticPartition is null ||
             !string.Equals(semanticPartition.Evidence.SourceSnapshotHash, snapshot.CanonicalHash, StringComparison.Ordinal)))
        {
            return Failure(Ra2VoxelViewportSceneFailureKind.InvalidRegionMask, "结构分区与当前模型不匹配。");
        }
        if (colourMode == Ra2VoxelViewportColourMode.SemanticMask &&
            (semanticComposition is null && (semanticEvidence is null || semanticAssignments is null) ||
             semanticComposition is not null &&
             (!string.Equals(semanticComposition.SourceSnapshotHash, snapshot.CanonicalHash, StringComparison.Ordinal) ||
              semanticComposition.CellCount != snapshot.OccupancyCount) ||
             semanticComposition is null && semanticEvidence is not null &&
             !string.Equals(semanticEvidence.SourceSnapshotHash, snapshot.CanonicalHash, StringComparison.Ordinal)))
        {
            return Failure(Ra2VoxelViewportSceneFailureKind.InvalidRegionMask, "语义掩码与当前模型不匹配。");
        }

        try
        {
            Ra2VoxelSceneSnapshot displaySnapshot = colourMode == Ra2VoxelViewportColourMode.Difference
                ? BuildDifferenceSnapshot(snapshot, comparisonSnapshot!)
                : snapshot;
            var projectionResult = Ra2VoxelSurfaceProjector.Project(displaySnapshot, maximumFaceCount, cancellationToken);
            if (!projectionResult.IsSuccess || projectionResult.Projection is null)
            {
                return projectionResult.FailureKind switch
                {
                    Ra2VoxelSurfaceProjectionFailureKind.ResourceLimitExceeded =>
                        Failure(Ra2VoxelViewportSceneFailureKind.ResourceLimitExceeded, "模型外露面数量超过 3D 审阅上限。"),
                    Ra2VoxelSurfaceProjectionFailureKind.Cancelled =>
                        Failure(Ra2VoxelViewportSceneFailureKind.Cancelled, "3D 场景生成已取消。"),
                    _ => Failure(Ra2VoxelViewportSceneFailureKind.AnalysisFailed, "无法生成模型的 3D 外露面。")
                };
            }

            Dictionary<Ra2VoxelCoordinate, int>? cellIndices = colourMode == Ra2VoxelViewportColourMode.GeometryRegion
                ? snapshot.Cells.Select((cell, index) => (cell.Coordinate, index))
                    .ToDictionary(item => item.Coordinate, item => item.index)
                : null;
            if (colourMode == Ra2VoxelViewportColourMode.SemanticMask)
                cellIndices = snapshot.Cells.Select((cell, index) => (cell.Coordinate, index))
                    .ToDictionary(item => item.Coordinate, item => item.index);
            Ra2VoxelSemanticEffectiveAssignment?[]? semanticByCell = colourMode == Ra2VoxelViewportColourMode.SemanticMask
                ? semanticComposition is not null
                    ? semanticComposition.Assignments.Cast<Ra2VoxelSemanticEffectiveAssignment?>().ToArray()
                    : BuildSemanticCellAssignments(snapshot, semanticEvidence!, semanticAssignments!)
                : null;
            HashSet<Ra2VoxelCoordinate>? candidateCoordinates = colourMode == Ra2VoxelViewportColourMode.Difference
                ? snapshot.Cells.Select(cell => cell.Coordinate).ToHashSet()
                : null;
            Dictionary<Ra2VoxelCoordinate, int>? comparisonIndices = colourMode == Ra2VoxelViewportColourMode.Difference
                ? comparisonSnapshot!.Cells.Select((cell, index) => (cell.Coordinate, index))
                    .ToDictionary(value => value.Coordinate, value => value.index)
                : null;

            Dictionary<uint, MeshBatch> batches = [];
            int processedFaces = 0;
            foreach (Ra2VoxelSurfaceFace face in projectionResult.Projection.Faces)
            {
                if ((processedFaces++ & 4095) == 0)
                    cancellationToken.ThrowIfCancellationRequested();

                Ra2Rgba32 rgba = colourMode switch
                {
                    Ra2VoxelViewportColourMode.Palette => displaySnapshot.Palette[face.PaletteIndex],
                    Ra2VoxelViewportColourMode.GeometryRegion =>
                        Ra2VoxelColourReviewPackageBuilder.GeometryRegionColour(geometryMask![cellIndices![face.Coordinate]]),
                    Ra2VoxelViewportColourMode.Difference => DifferenceColour(
                        face.Coordinate,
                        candidateCoordinates!,
                        comparisonIndices!),
                    Ra2VoxelViewportColourMode.SemanticStructure => SemanticStructureColour(
                        semanticPartition!.DispositionAt(face.Coordinate)),
                    Ra2VoxelViewportColourMode.SemanticMask => SemanticMaskColour(
                        semanticByCell![cellIndices![face.Coordinate]], semanticReviewDimension),
                    _ => throw new ArgumentOutOfRangeException(nameof(colourMode))
                };
                uint key = ((uint)rgba.Alpha << 24) | ((uint)rgba.Red << 16) | ((uint)rgba.Green << 8) | rgba.Blue;
                if (!batches.TryGetValue(key, out MeshBatch? batch))
                {
                    batch = new MeshBatch(rgba);
                    batches.Add(key, batch);
                }
                AppendFace(batch, face, displaySnapshot);
            }

            Model3DGroup group = new();
            Dictionary<GeometryModel3D, IReadOnlyList<Ra2VoxelCoordinate>> hitFaces =
                new(ReferenceEqualityComparer.Instance);
            foreach (MeshBatch batch in batches.OrderBy(pair => pair.Key).Select(pair => pair.Value))
            {
                GeometryModel3D model = CreateGeometryModel(batch);
                group.Children.Add(model);
                hitFaces.Add(model, batch.FaceCoordinates.ToArray());
            }
            group.Freeze();
            Ra2VoxelViewportSceneHitMap hitMap = new(hitFaces);

            Rect3D bounds = new(
                -displaySnapshot.Part.XSize / 2d,
                -displaySnapshot.Part.YSize / 2d,
                -displaySnapshot.Part.ZSize / 2d,
                displaySnapshot.Part.XSize,
                displaySnapshot.Part.YSize,
                displaySnapshot.Part.ZSize);
            return new(
                Ra2VoxelViewportSceneFailureKind.None,
                string.Empty,
                group,
                hitMap,
                bounds,
                projectionResult.Projection.FaceCount,
                batches.Count);
        }
        catch (OperationCanceledException)
        {
            return Failure(Ra2VoxelViewportSceneFailureKind.Cancelled, "3D 场景生成已取消。");
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or OverflowException)
        {
            return Failure(Ra2VoxelViewportSceneFailureKind.AnalysisFailed, "3D 场景数据不完整，已保留切片预览。 ");
        }
    }

    private static Ra2VoxelSceneSnapshot BuildDifferenceSnapshot(
        Ra2VoxelSceneSnapshot candidate,
        Ra2VoxelSceneSnapshot comparison)
    {
        Dictionary<Ra2VoxelCoordinate, byte> cells = comparison.Cells
            .ToDictionary(cell => cell.Coordinate, cell => cell.PaletteIndex);
        foreach (Ra2VoxelCell cell in candidate.Cells)
            cells[cell.Coordinate] = cell.PaletteIndex;
        return new(
            candidate.SceneId,
            candidate.Part,
            candidate.Palette,
            cells.Select(value => new Ra2VoxelCell(value.Key, value.Value)),
            candidate.SourceArtifactHashes);
    }

    private static Ra2Rgba32 DifferenceColour(
        Ra2VoxelCoordinate coordinate,
        IReadOnlySet<Ra2VoxelCoordinate> candidate,
        IReadOnlyDictionary<Ra2VoxelCoordinate, int> comparisonIndices)
    {
        bool inCandidate = candidate.Contains(coordinate);
        bool inComparison = comparisonIndices.ContainsKey(coordinate);
        if (inCandidate && !inComparison) return new(64, 170, 92);
        if (!inCandidate && inComparison) return new(214, 72, 72);
        return new(128, 135, 146, 52);
    }

    private static Ra2Rgba32 SemanticStructureColour(Ra2VoxelSymmetryDisposition disposition) => disposition switch
    {
        Ra2VoxelSymmetryDisposition.SymmetricCore => new(35, 190, 196),
        Ra2VoxelSymmetryDisposition.AsymmetricAttachment => new(222, 155, 48),
        Ra2VoxelSymmetryDisposition.ProtectedThinFeature => new(61, 126, 220),
        Ra2VoxelSymmetryDisposition.Uncertain => new(148, 95, 210),
        _ => new(128, 135, 146)
    };

    private static Ra2VoxelSemanticEffectiveAssignment?[] BuildSemanticCellAssignments(
        Ra2VoxelSceneSnapshot snapshot,
        Ra2VoxelSemanticEvidencePackage evidence,
        IReadOnlyList<Ra2VoxelSemanticEffectiveAssignment> assignments)
    {
        Dictionary<string, Ra2VoxelSemanticEffectiveAssignment> byRegion = assignments.ToDictionary(value => value.RegionId, StringComparer.Ordinal);
        Ra2VoxelSemanticEffectiveAssignment?[] cells = new Ra2VoxelSemanticEffectiveAssignment?[snapshot.OccupancyCount];
        foreach (var region in evidence.Regions)
        {
            if (!byRegion.TryGetValue(region.RegionId, out Ra2VoxelSemanticEffectiveAssignment? assignment)) continue;
            for (int index = 0; index < region.Selected.Count; index++)
                if (region.Selected[index] != 0) cells[index] = assignment;
        }
        return cells;
    }

    private static Ra2Rgba32 SemanticMaskColour(
        Ra2VoxelSemanticEffectiveAssignment? assignment,
        Ra2VoxelSemanticReviewDimension dimension) => dimension == Ra2VoxelSemanticReviewDimension.Part
            ? Ra2VoxelSemanticReviewPalette.PartColour(assignment?.PartRole ?? Ra2VoxelSemanticPartRole.Unknown)
            : Ra2VoxelSemanticReviewPalette.MaterialColour(assignment?.MaterialRole ?? Ra2VoxelSemanticMaterialRole.Unknown);

    private static bool HasSameGrid(Ra2VoxelSceneSnapshot left, Ra2VoxelSceneSnapshot right) =>
        left.Part.XSize == right.Part.XSize && left.Part.YSize == right.Part.YSize &&
        left.Part.ZSize == right.Part.ZSize;

    private static void AppendFace(MeshBatch batch, Ra2VoxelSurfaceFace face, Ra2VoxelSceneSnapshot snapshot)
    {
        double x0 = face.Coordinate.X - (snapshot.Part.XSize / 2d);
        double y0 = face.Coordinate.Y - (snapshot.Part.YSize / 2d);
        double z0 = face.Coordinate.Z - (snapshot.Part.ZSize / 2d);
        double x1 = x0 + 1d;
        double y1 = y0 + 1d;
        double z1 = z0 + 1d;
        (Point3D A, Point3D B, Point3D C, Point3D D, Vector3D Normal) quad = face.Direction switch
        {
            Ra2VoxelFaceDirection.NegativeX => (new(x0, y0, z0), new(x0, y0, z1), new(x0, y1, z1), new(x0, y1, z0), new(-1, 0, 0)),
            Ra2VoxelFaceDirection.PositiveX => (new(x1, y0, z0), new(x1, y1, z0), new(x1, y1, z1), new(x1, y0, z1), new(1, 0, 0)),
            Ra2VoxelFaceDirection.NegativeY => (new(x0, y0, z0), new(x1, y0, z0), new(x1, y0, z1), new(x0, y0, z1), new(0, -1, 0)),
            Ra2VoxelFaceDirection.PositiveY => (new(x0, y1, z0), new(x0, y1, z1), new(x1, y1, z1), new(x1, y1, z0), new(0, 1, 0)),
            Ra2VoxelFaceDirection.NegativeZ => (new(x0, y0, z0), new(x0, y1, z0), new(x1, y1, z0), new(x1, y0, z0), new(0, 0, -1)),
            Ra2VoxelFaceDirection.PositiveZ => (new(x0, y0, z1), new(x1, y0, z1), new(x1, y1, z1), new(x0, y1, z1), new(0, 0, 1)),
            _ => throw new ArgumentOutOfRangeException(nameof(face.Direction))
        };

        int first = batch.Positions.Count;
        batch.Positions.Add(quad.A);
        batch.Positions.Add(quad.B);
        batch.Positions.Add(quad.C);
        batch.Positions.Add(quad.D);
        for (int index = 0; index < 4; index++)
            batch.Normals.Add(quad.Normal);
        batch.Indices.Add(first);
        batch.Indices.Add(first + 1);
        batch.Indices.Add(first + 2);
        batch.Indices.Add(first);
        batch.Indices.Add(first + 2);
        batch.Indices.Add(first + 3);
        batch.FaceCoordinates.Add(face.Coordinate);
        batch.FaceCount++;
    }

    internal static Model3DGroup BuildCoordinateOverlay(
        Ra2VoxelSceneSnapshot snapshot,
        IReadOnlyCollection<Ra2VoxelCoordinate> coordinates,
        Ra2Rgba32 colour)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(coordinates);
        if (coordinates.Count > 8192)
            throw new ArgumentOutOfRangeException(nameof(coordinates));
        HashSet<Ra2VoxelCoordinate> occupied = snapshot.Cells.Select(cell => cell.Coordinate).ToHashSet();
        MeshBatch batch = new(colour);
        foreach (Ra2VoxelCoordinate coordinate in coordinates.Distinct())
        {
            if (!occupied.Contains(coordinate))
                throw new ArgumentException("Stroke preview coordinates must belong to the current snapshot.", nameof(coordinates));
            foreach (Ra2VoxelFaceDirection direction in Enum.GetValues<Ra2VoxelFaceDirection>())
                AppendFace(batch, new(coordinate, direction, 0), snapshot);
        }

        Model3DGroup group = new();
        if (batch.FaceCount > 0)
            group.Children.Add(CreateGeometryModel(batch, emissive: true));
        group.Freeze();
        return group;
    }

    private static GeometryModel3D CreateGeometryModel(MeshBatch batch, bool emissive = false)
    {
        Point3DCollection positions = new(batch.Positions);
        Vector3DCollection normals = new(batch.Normals);
        Int32Collection indices = new(batch.Indices);
        positions.Freeze();
        normals.Freeze();
        indices.Freeze();
        MeshGeometry3D mesh = new()
        {
            Positions = positions,
            Normals = normals,
            TriangleIndices = indices
        };
        mesh.Freeze();

        Color colour = Color.FromArgb(batch.Colour.Alpha, batch.Colour.Red, batch.Colour.Green, batch.Colour.Blue);
        SolidColorBrush brush = new(colour);
        brush.Freeze();
        MaterialGroup material = new();
        material.Children.Add(new DiffuseMaterial(brush));
        if (emissive)
            material.Children.Add(new EmissiveMaterial(brush));
        SolidColorBrush highlightBrush = new(Color.FromArgb(44, 255, 255, 255));
        highlightBrush.Freeze();
        material.Children.Add(new SpecularMaterial(highlightBrush, 14d));
        material.Freeze();
        GeometryModel3D model = new(mesh, material) { BackMaterial = material };
        model.Freeze();
        return model;
    }

    private static Ra2VoxelViewportSceneBuildResult Failure(Ra2VoxelViewportSceneFailureKind kind, string message) =>
        new(kind, message, null, Ra2VoxelViewportSceneHitMap.Empty, Rect3D.Empty, 0, 0);

    private sealed class MeshBatch(Ra2Rgba32 colour)
    {
        internal Ra2Rgba32 Colour { get; } = colour;
        internal List<Point3D> Positions { get; } = [];
        internal List<Vector3D> Normals { get; } = [];
        internal List<int> Indices { get; } = [];
        internal List<Ra2VoxelCoordinate> FaceCoordinates { get; } = [];
        internal int FaceCount { get; set; }
    }
}
