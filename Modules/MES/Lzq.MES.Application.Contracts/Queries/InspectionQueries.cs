using Lzq.Core.Models;
using Lzq.MES.Domain.Enums;

namespace Lzq.MES.Application.Contracts.Queries;

/// <summary>
/// 点检计划分页查询
/// </summary>
public record InspectionPlanPageQuery : PagedRequest
{
    /// <summary>点检计划编号</summary>
    public string? Code { get; init; }

    /// <summary>点检计划名称</summary>
    public string? Name { get; init; }

    /// <summary>设备ID</summary>
    public long? EquipmentId { get; init; }

    /// <summary>是否启用</summary>
    public bool? IsEnabled { get; init; }

    /// <summary>下次点检日期（起）</summary>
    public DateTime? NextInspectDateFrom { get; init; }

    /// <summary>下次点检日期（止）</summary>
    public DateTime? NextInspectDateTo { get; init; }
}

/// <summary>
/// 点检记录分页查询
/// </summary>
public record InspectionRecordPageQuery : PagedRequest
{
    /// <summary>点检记录编号</summary>
    public string? Code { get; init; }

    /// <summary>设备ID</summary>
    public long? EquipmentId { get; init; }

    /// <summary>设备编号</summary>
    public string? EquipmentCode { get; init; }

    /// <summary>设备名称</summary>
    public string? EquipmentName { get; init; }

    /// <summary>点检结果</summary>
    public InspectionResultEnum? Result { get; init; }

    /// <summary>检验员ID</summary>
    public long? InspectorId { get; init; }

    /// <summary>点检日期（起）</summary>
    public DateTime? InspectDateFrom { get; init; }

    /// <summary>点检日期（止）</summary>
    public DateTime? InspectDateTo { get; init; }
}
