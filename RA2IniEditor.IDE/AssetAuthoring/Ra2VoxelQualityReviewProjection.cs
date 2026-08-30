extern alias Ra2Application;

using Ra2VoxelGeometryQualityFacts = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelGeometryQualityFacts;
using Ra2VoxelRefinementReviewPackage = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelRefinementReviewPackage;
using Ra2VoxelSceneSnapshot = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSceneSnapshot;
using Ra2VoxelSemanticRegionProposal = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticRegionProposal;
using Ra2VoxelSemanticPartition = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticPartition;
using Ra2VoxelSymmetryDisposition = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSymmetryDisposition;

namespace RA2IniEditor.IDE.AssetAuthoring;

internal sealed record Ra2VoxelQualityMetricRow(
    string Label,
    string Current,
    string Direct,
    string Refined,
    string Symmetry);

internal sealed record Ra2VoxelSemanticRegionRow(
    string Region,
    string Disposition,
    string Provenance,
    string Confidence,
    string CellCount,
    string ReviewNote);

internal static class Ra2VoxelQualityReviewProjection
{
    internal static IReadOnlyList<Ra2VoxelQualityMetricRow> ProjectMetrics(
        Ra2VoxelGeometryQualityFacts baseline,
        Ra2VoxelRefinementReviewPackage review,
        Ra2VoxelSceneSnapshot baselineSnapshot,
        Ra2VoxelSceneSnapshot directSnapshot,
        Ra2VoxelSceneSnapshot refinedSnapshot,
        Ra2VoxelSceneSnapshot? symmetrySnapshot)
    {
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(review);
        ArgumentNullException.ThrowIfNull(baselineSnapshot);
        ArgumentNullException.ThrowIfNull(directSnapshot);
        ArgumentNullException.ThrowIfNull(refinedSnapshot);
        return
        [
            Row("占用体素", baseline, review, facts => facts.OccupiedCellCount.ToString("N0")),
            SnapshotRow("组件数", baselineSnapshot, directSnapshot, refinedSnapshot, symmetrySnapshot,
                snapshot => snapshot.Connectivity.ComponentCount.ToString("N0")),
            SnapshotRow("主体占比", baselineSnapshot, directSnapshot, refinedSnapshot, symmetrySnapshot,
                DominantComponentShare),
            Row("表面粗糙度", baseline, review, facts => facts.RoughnessScore.ToString("F3")),
            Row("低支撑表面", baseline, review, facts => facts.LowSupportSurfaceCellCount.ToString("N0")),
            Row("X 对称度", baseline, review, facts => facts.SymmetryScore.ToString("P1")),
            Row("未匹配体素", baseline, review, facts => facts.UnmatchedCellCount.ToString("N0"))
        ];
    }

    private static Ra2VoxelQualityMetricRow SnapshotRow(
        string label,
        Ra2VoxelSceneSnapshot baseline,
        Ra2VoxelSceneSnapshot direct,
        Ra2VoxelSceneSnapshot refined,
        Ra2VoxelSceneSnapshot? symmetry,
        Func<Ra2VoxelSceneSnapshot, string> formatter) =>
        new(label, formatter(baseline), formatter(direct), formatter(refined),
            symmetry is null ? "不可用" : formatter(symmetry));

    private static string DominantComponentShare(Ra2VoxelSceneSnapshot snapshot) =>
        snapshot.OccupancyCount == 0
            ? "0.0%"
            : (snapshot.Connectivity.LargestComponentCellCount / (double)snapshot.OccupancyCount).ToString("P1");

    internal static IReadOnlyList<Ra2VoxelSemanticRegionRow> ProjectSemanticRegions(
        Ra2VoxelRefinementReviewPackage review)
    {
        ArgumentNullException.ThrowIfNull(review);
        return review.SemanticRegions
            .Select(ProjectSemanticRegion)
            .ToArray();
    }

    internal static IReadOnlyList<Ra2VoxelSemanticRegionRow> ProjectSemanticRegions(
        Ra2VoxelSemanticPartition partition)
    {
        ArgumentNullException.ThrowIfNull(partition);
        Dictionary<string, int> counts = partition.Evidence.Regions
            .ToDictionary(value => value.RegionId, value => value.CellCount, StringComparer.Ordinal);
        return partition.Decisions.Select(decision => new Ra2VoxelSemanticRegionRow(
            decision.RegionId,
            DispositionName(decision.Disposition),
            decision.RoundsAgree ? "Agent 最终稀疏提案" : "未选择 → 原位保留",
            decision.RoundsAgree
                ? Math.Min(decision.RoundOneConfidence, decision.RoundTwoConfidence).ToString("P0")
                : "—",
            counts[decision.RegionId].ToString("N0"),
            decision.ReviewReason)).ToArray();
    }

    internal static string ProjectNormalComparison(Ra2VoxelRefinementReviewPackage review)
    {
        ArgumentNullException.ThrowIfNull(review);
        var facts = review.NormalComparison;
        return $"共同坐标 {facts.CommonCoordinateCount:N0} · 法线变化 {facts.ChangedNormalIndexCount:N0} · " +
            $"直接 {facts.SourceSampleCount:N0} / 平滑 {facts.CandidateSampleCount:N0} 个采样";
    }

    internal static string ProjectCandidateReviews(Ra2VoxelRefinementReviewPackage review)
    {
        ArgumentNullException.ThrowIfNull(review);
        return string.Join("；", review.CandidateReviews.Select(candidate =>
        {
            string name = candidate.CandidateId switch
            {
                "Conservative" => "保守平滑",
                "Balanced" => "平衡平滑",
                "SurfacePolish" => "强表面清理",
                _ => candidate.CandidateId
            };
            string state = candidate.IsSelected
                ? "已采用"
                : candidate.IsSafe
                    ? "仅审阅"
                    : "未通过门禁";
            string facts = candidate.Facts is { } quality
                ? $"粗糙度 {quality.RoughnessScore:F3}，低支撑 {quality.LowSupportSurfaceCellCount:N0}"
                : "质量事实不可用";
            string reason = candidate.IsSelected
                ? "达到平滑目标"
                : candidate.IsSafe
                    ? "仅供比较，未达到自动平滑阈值"
                    : "未通过结构安全门禁";
            return $"{name}：{state}，+{candidate.AddedCellCount:N0}/-{candidate.RemovedCellCount:N0}，{facts}，{reason}";
        }));
    }

    private static Ra2VoxelQualityMetricRow Row(
        string label,
        Ra2VoxelGeometryQualityFacts baseline,
        Ra2VoxelRefinementReviewPackage review,
        Func<Ra2VoxelGeometryQualityFacts, string> formatter) =>
        new(
            label,
            formatter(baseline),
            formatter(review.SourceFacts),
            formatter(review.RefinedFacts),
            review.SymmetryFacts is null ? "不可用" : formatter(review.SymmetryFacts));

    private static Ra2VoxelSemanticRegionRow ProjectSemanticRegion(Ra2VoxelSemanticRegionProposal region) =>
        new(
            RegionName(region.RegionId),
            "待 AI 识别",
            ProvenanceName(region.Provenance.ToString()),
            region.Confidence.ToString("P0"),
            region.CellCount.ToString("N0"),
            region.ReviewNote);

    private static string DispositionName(Ra2VoxelSymmetryDisposition value) => value switch
    {
        Ra2VoxelSymmetryDisposition.SymmetricCore => "镜像补全",
        Ra2VoxelSymmetryDisposition.AsymmetricAttachment => "计划移除",
        Ra2VoxelSymmetryDisposition.ProtectedThinFeature => "受保护薄结构",
        Ra2VoxelSymmetryDisposition.Uncertain => "未选择/保留",
        _ => value.ToString()
    };

    private static string RegionName(string value) => value switch
    {
        "body-shell" => "车体外壳",
        "lower-contact-candidate" => "下部接地区域候选",
        "upper-aperture-candidate" => "上部开口区域候选",
        "protected-thin-structures" => "受保护薄结构",
        _ => value
    };

    private static string ProvenanceName(string value) => value switch
    {
        "GeometryVerified" => "几何确认",
        "UserDeclared" => "用户声明",
        "ModelInferred" => "模型推断",
        "VisionVerified" => "视觉确认",
        "Unresolved" => "未解析",
        _ => value
    };
}
