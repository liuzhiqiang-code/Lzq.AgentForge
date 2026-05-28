namespace Lzq.MES.Application.Contracts.WorkOrder.Commands;

/// <summary>
/// 开工命令
/// </summary>
public record WorkOrderStartCommand
{
    /// <summary>工单ID</summary>
    public long Id { get; init; }
}
