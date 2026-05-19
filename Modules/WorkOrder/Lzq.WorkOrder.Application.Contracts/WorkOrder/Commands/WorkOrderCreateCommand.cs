using Lzq.WorkOrder.Domain.Enums;

namespace Lzq.WorkOrder.Application.Contracts.WorkOrder.Commands;

/// <summary>
/// 创建工单命令
/// </summary>
public record WorkOrderCreateCommand
{
    /// <summary>工单编号</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>产品名称</summary>
    public string ProductName { get; init; } = string.Empty;

    /// <summary>产品规格/型号</summary>
    public string? ProductSpec { get; init; }

    /// <summary>产线ID</summary>
    public long LineId { get; init; }

    /// <summary>工序ID</summary>
    public long ProcessId { get; init; }

    /// <summary>计划数量</summary>
    public int PlannedQty { get; init; }

    /// <summary>计划开始时间</summary>
    public DateTime? PlannedStart { get; init; }

    /// <summary>计划结束时间</summary>
    public DateTime? PlannedEnd { get; init; }

    /// <summary>优先级（1-5，5最高）</summary>
    public int Priority { get; init; } = 3;

    /// <summary>备注</summary>
    public string? Remark { get; init; }
}
