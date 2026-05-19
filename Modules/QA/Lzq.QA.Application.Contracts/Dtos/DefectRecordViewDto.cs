using Lzq.QA.Domain.Enums;

namespace Lzq.QA.Application.Contracts.Dtos;

/// <summary>
/// 不良品记录视图DTO
/// </summary>
public class DefectRecordViewDto
{
    public long Id { get; set; }
    public long? QCOrderId { get; set; }
    public string? QCOrderCode { get; set; }
    public long? WorkOrderId { get; set; }
    public string? WorkOrderCode { get; set; }
    public long? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductSpec { get; set; }
    public string? BatchNo { get; set; }
    public int DefectQty { get; set; }
    public string DefectCode { get; set; } = string.Empty;
    public string DefectDesc { get; set; } = string.Empty;
    public string? DefectImages { get; set; }
    public DefectStatusEnum Status { get; set; }
    public string? StatusName { get; set; }
    public DefectHandlingEnum? HandlingType { get; set; }
    public string? HandlingTypeName { get; set; }
    public string? HandlingRemark { get; set; }
    public long? HandlerId { get; set; }
    public string? HandlerName { get; set; }
    public DateTime? HandlingTime { get; set; }
    public bool NeedReview { get; set; }
    public string? ReviewResult { get; set; }
    public string? Reviewer { get; set; }
    public DateTime? ReviewTime { get; set; }
    public string? Remark { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? UpdateTime { get; set; }
}

/// <summary>
/// 不良品统计DTO
/// </summary>
public class DefectStatisticsDto
{
    public int TotalDefectCount { get; set; }
    public int ReworkCount { get; set; }
    public int ScrapCount { get; set; }
    public int DowngradeCount { get; set; }
    public int ReturnCount { get; set; }
    public int PendingCount { get; set; }
    public int ProcessingCount { get; set; }
    public int ProcessedCount { get; set; }
}

