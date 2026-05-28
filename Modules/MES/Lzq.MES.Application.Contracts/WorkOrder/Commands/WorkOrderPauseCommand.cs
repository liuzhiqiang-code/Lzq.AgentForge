namespace Lzq.MES.Application.Contracts.WorkOrder.Commands;

/// <summary>
/// 暂停工单命令
/// </summary>
public record WorkOrderPauseCommand
{
    /// <summary>工单ID</summary>
    public long Id { get; init; }

    /// <summary>暂停原因</summary>
    public string? Reason { get; init; }
}
