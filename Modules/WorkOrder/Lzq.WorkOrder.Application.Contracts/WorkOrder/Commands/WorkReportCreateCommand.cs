namespace Lzq.WorkOrder.Application.Contracts.WorkOrder.Commands;

/// <summary>
/// 创建报工记录命令
/// </summary>
public record WorkReportCreateCommand
{
    /// <summary>工单ID</summary>
    public long WorkOrderId { get; init; }

    /// <summary>操作人员ID</summary>
    public string? OperatorId { get; init; }

    /// <summary>操作人员名称</summary>
    public string? OperatorName { get; init; }

    /// <summary>合格数量</summary>
    public int QualifiedQty { get; init; }

    /// <summary>不良数量</summary>
    public int DefectQty { get; init; }

    /// <summary>工时（小时）</summary>
    public decimal WorkHours { get; init; }

    /// <summary>班次</summary>
    public string? Shift { get; init; }

    /// <summary>备注</summary>
    public string? Remark { get; init; }
}
