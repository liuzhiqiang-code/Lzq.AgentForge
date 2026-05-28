using Lzq.MES.Domain.Enums;

namespace Lzq.MES.Application.Contracts.Dtos;

public record EcnDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EcnStatusEnum Status { get; set; }
    public string? Reason { get; set; }
    public string? ImpactAnalysis { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public string? Remark { get; set; }
    public DateTime CreationTime { get; set; }
}

public record EcnDetailDto
{
    public EcnDto Header { get; set; } = null!;
    public List<EcnItemDto> Items { get; set; } = new();
}

public record EcnItemDto
{
    public long Id { get; set; }
    public long EcnId { get; set; }
    public EcnChangeTypeEnum ChangeType { get; set; }
    public long TargetId { get; set; }
    public string? TargetCode { get; set; }
    public string? TargetName { get; set; }
    public string? ChangeSummary { get; set; }
    public string? BeforeSnapshot { get; set; }
    public string? AfterSnapshot { get; set; }
    public string? Remark { get; set; }
}

public record BomVersionHistoryDto
{
    public long Id { get; set; }
    public long BomId { get; set; }
    public string Version { get; set; } = string.Empty;
    public string? ChangeDescription { get; set; }
    public DateTime CreationTime { get; set; }
}

public record BomDiffResultDto
{
    public string OldVersion { get; set; } = string.Empty;
    public string NewVersion { get; set; } = string.Empty;
    public string HeaderDiff { get; set; } = string.Empty;
    public List<BomItemDiffDto> ItemChanges { get; set; } = new();
}

public record BomItemDiffDto
{
    public string ChangeType { get; set; } = string.Empty;
    public long ItemId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public decimal? OldQty { get; set; }
    public decimal? NewQty { get; set; }
    public decimal? OldScrapRate { get; set; }
    public decimal? NewScrapRate { get; set; }
}

