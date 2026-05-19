using Lzq.Core.Models;
using Lzq.WorkOrder.Domain.Enums;

namespace Lzq.WorkOrder.Application.Contracts.WorkOrder.Queries;

/// <summary>
/// 工单分页查询
/// </summary>
public record WorkOrderPageQuery : PagedRequest
{
    /// <summary>工单编号</summary>
    public string? Code { get; init; }

    /// <summary>产品名称</summary>
    public string? ProductName { get; init; }

    /// <summary>产线ID</summary>
    public long? LineId { get; init; }

    /// <summary>工序ID</summary>
    public long? ProcessId { get; init; }

    /// <summary>工单状态</summary>
    public WorkOrderStatusEnum? Status { get; init; }

    /// <summary>计划开始时间（起）</summary>
    public DateTime? PlannedStartFrom { get; init; }

    /// <summary>计划开始时间（止）</summary>
    public DateTime? PlannedStartTo { get; init; }

    /// <summary>创建时间（起）</summary>
    public DateTime? CreateTimeFrom { get; init; }

    /// <summary>创建时间（止）</summary>
    public DateTime? CreateTimeTo { get; init; }
}
