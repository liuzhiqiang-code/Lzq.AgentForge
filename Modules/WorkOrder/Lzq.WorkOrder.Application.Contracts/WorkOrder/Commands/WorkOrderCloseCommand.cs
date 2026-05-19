namespace Lzq.WorkOrder.Application.Contracts.WorkOrder.Commands;

/// <summary>
/// 关闭工单命令
/// </summary>
public record WorkOrderCloseCommand
{
    /// <summary>工单ID</summary>
    public long Id { get; init; }
}
