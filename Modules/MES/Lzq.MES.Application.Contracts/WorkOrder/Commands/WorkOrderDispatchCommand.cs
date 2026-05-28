namespace Lzq.MES.Application.Contracts.WorkOrder.Commands;

/// <summary>
/// 派发工单命令
/// </summary>
public record WorkOrderDispatchCommand
{
    /// <summary>工单ID</summary>
    public long Id { get; init; }
}
