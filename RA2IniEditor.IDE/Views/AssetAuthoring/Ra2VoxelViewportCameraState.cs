using System.Windows.Media.Media3D;

namespace RA2IniEditor.IDE.Views.AssetAuthoring;

internal readonly record struct Ra2VoxelViewportCameraState(
    double Yaw,
    double Pitch,
    double NormalizedTargetX,
    double NormalizedTargetY,
    double NormalizedTargetZ,
    double DistanceRatio,
    bool HasUserInteraction)
{
    private const double MinimumDistanceRatio = 0.25d;
    private const double MaximumDistanceRatio = 12d;

    internal static bool TryCapture(
        Rect3D bounds,
        Point3D target,
        double distance,
        double yaw,
        double pitch,
        bool hasUserInteraction,
        out Ra2VoxelViewportCameraState state)
    {
        state = default;
        double diagonal = BoundsDiagonal(bounds);
        if (bounds.IsEmpty || diagonal <= 0d ||
            !IsFinite(target.X) || !IsFinite(target.Y) || !IsFinite(target.Z) ||
            !IsFinite(distance) || distance <= 0d || !IsFinite(yaw) || !IsFinite(pitch))
        {
            return false;
        }

        double normalizedX = Normalize(target.X, bounds.X, bounds.SizeX);
        double normalizedY = Normalize(target.Y, bounds.Y, bounds.SizeY);
        double normalizedZ = Normalize(target.Z, bounds.Z, bounds.SizeZ);
        double distanceRatio = distance / diagonal;
        if (!IsFinite(normalizedX) || !IsFinite(normalizedY) || !IsFinite(normalizedZ) ||
            !IsFinite(distanceRatio))
        {
            return false;
        }

        state = new(
            yaw,
            pitch,
            Math.Clamp(normalizedX, 0d, 1d),
            Math.Clamp(normalizedY, 0d, 1d),
            Math.Clamp(normalizedZ, 0d, 1d),
            Math.Clamp(distanceRatio, MinimumDistanceRatio, MaximumDistanceRatio),
            hasUserInteraction);
        return true;
    }

    internal bool TryRestore(
        Rect3D bounds,
        out Point3D target,
        out double distance,
        out double yaw,
        out double pitch)
    {
        target = default;
        distance = default;
        yaw = default;
        pitch = default;
        double diagonal = BoundsDiagonal(bounds);
        if (bounds.IsEmpty || diagonal <= 0d ||
            !IsFinite(Yaw) || !IsFinite(Pitch) ||
            !IsUnitValue(NormalizedTargetX) || !IsUnitValue(NormalizedTargetY) || !IsUnitValue(NormalizedTargetZ) ||
            !IsFinite(DistanceRatio) || DistanceRatio < MinimumDistanceRatio || DistanceRatio > MaximumDistanceRatio)
        {
            return false;
        }

        target = new(
            Denormalize(NormalizedTargetX, bounds.X, bounds.SizeX),
            Denormalize(NormalizedTargetY, bounds.Y, bounds.SizeY),
            Denormalize(NormalizedTargetZ, bounds.Z, bounds.SizeZ));
        distance = diagonal * DistanceRatio;
        yaw = Yaw;
        pitch = Pitch;
        return IsFinite(target.X) && IsFinite(target.Y) && IsFinite(target.Z) &&
            IsFinite(distance) && distance > 0d;
    }

    private static double Normalize(double value, double origin, double size) =>
        size > 0d ? (value - origin) / size : 0.5d;

    private static double Denormalize(double value, double origin, double size) =>
        size > 0d ? origin + (value * size) : origin;

    private static double BoundsDiagonal(Rect3D bounds) => bounds.IsEmpty
        ? 0d
        : Math.Sqrt((bounds.SizeX * bounds.SizeX) +
            (bounds.SizeY * bounds.SizeY) +
            (bounds.SizeZ * bounds.SizeZ));

    private static bool IsUnitValue(double value) => IsFinite(value) && value is >= 0d and <= 1d;

    private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
}
