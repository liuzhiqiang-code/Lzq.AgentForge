using Lzq.MES.Domain.Enums;

namespace Lzq.MES.Application.Contracts.WorkOrder.Dto;

/// <summary>
/// 工单视图DTO
/// </summary>
public class WorkOrderViewDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSpec { get; set; }
    public long LineId { get; set; }
    public string? LineName { get; set; }
    public long ProcessId { get; set; }
    public string? ProcessName { get; set; }
    public int PlannedQty { get; set; }
    public int CompletedQty { get; set; }
    public int DefectQty { get; set; }
    public WorkOrderStatusEnum Status { get; set; }
    public string? StatusName { get; set; }
    public DateTime? PlannedStart { get; set; }
    public DateTime? PlannedEnd { get; set; }
    public DateTime? ActualStart { get; set; }
    public DateTime? ActualEnd { get; set; }
    public int Priority { get; set; }
    public string? Remark { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? UpdateTime { get; set; }
}
