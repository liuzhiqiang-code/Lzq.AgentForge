namespace Lzq.MES.Application.Contracts.WorkOrder.Commands;

/// <summary>
/// 完工命令
/// </summary>
public record WorkOrderCompleteCommand
{
    /// <summary>工单ID</summary>
    public long Id { get; init; }
}
