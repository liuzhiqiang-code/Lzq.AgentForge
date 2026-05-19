using Lzq.Core.Models;

namespace Lzq.WorkOrder.Application.Contracts.WorkOrder.Queries;

/// <summary>
/// 报工记录分页查询
/// </summary>
public record WorkReportPageQuery : PagedRequest
{
    /// <summary>工单ID</summary>
    public long? WorkOrderId { get; init; }

    /// <summary>操作人员ID</summary>
    public string? OperatorId { get; init; }

    /// <summary>报工时间（起）</summary>
    public DateTime? ReportTimeFrom { get; init; }

    /// <summary>报工时间（止）</summary>
    public DateTime? ReportTimeTo { get; init; }
}
