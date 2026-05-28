namespace Lzq.MES.Application.Contracts.WorkOrder.Dto;

/// <summary>
/// 报工记录视图DTO
/// </summary>
public class WorkReportViewDto
{
    public long Id { get; set; }
    public long WorkOrderId { get; set; }
    public string? WorkOrderCode { get; set; }
    public string? OperatorId { get; set; }
    public string? OperatorName { get; set; }
    public int QualifiedQty { get; set; }
    public int DefectQty { get; set; }
    public decimal WorkHours { get; set; }
    public DateTime ReportTime { get; set; }
    public string? Shift { get; set; }
    public string? Remark { get; set; }
    public DateTime CreateTime { get; set; }
}
