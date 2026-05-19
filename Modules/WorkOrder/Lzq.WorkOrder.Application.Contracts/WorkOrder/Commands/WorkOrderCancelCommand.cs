namespace Lzq.WorkOrder.Application.Contracts.WorkOrder.Commands;

/// <summary>
/// 取消工单命令
/// </summary>
public record WorkOrderCancelCommand
{
    /// <summary>工单ID</summary>
    public long Id { get; init; }

    /// <summary>取消原因</summary>
    public string? Reason { get; init; }
}
