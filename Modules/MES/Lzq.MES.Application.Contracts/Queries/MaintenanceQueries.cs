using Lzq.Core.Models;
using Lzq.MES.Domain.Enums;

namespace Lzq.MES.Application.Contracts.Queries;

/// <summary>
/// 报修单分页查询
/// </summary>
public record RepairOrderPageQuery : PagedRequest
{
    /// <summary>报修单编号</summary>
    public string? Code { get; init; }

    /// <summary>设备ID</summary>
    public long? EquipmentId { get; init; }

    /// <summary>设备编号</summary>
    public string? EquipmentCode { get; init; }

    /// <summary>设备名称</summary>
    public string? EquipmentName { get; init; }

    /// <summary>报修类型</summary>
    public int? RepairType { get; init; }

    /// <summary>维修状态</summary>
    public RepairStatusEnum? Status { get; init; }

    /// <summary>优先级</summary>
    public RepairPriorityEnum? Priority { get; init; }

    /// <summary>报修人ID</summary>
    public long? ReporterId { get; init; }

    /// <summary>维修人员ID</summary>
    public long? RepairUserId { get; init; }

    /// <summary>报修时间（起）</summary>
    public DateTime? ReportTimeFrom { get; init; }

    /// <summary>报修时间（止）</summary>
    public DateTime? ReportTimeTo { get; init; }
}

/// <summary>
/// 保养计划分页查询
/// </summary>
public record MaintenancePlanPageQuery : PagedRequest
{
    /// <summary>保养计划编号</summary>
    public string? Code { get; init; }

    /// <summary>保养计划名称</summary>
    public string? Name { get; init; }

    /// <summary>设备ID</summary>
    public long? EquipmentId { get; init; }

    /// <summary>保养类型</summary>
    public MaintenanceTypeEnum? MaintenanceType { get; init; }

    /// <summary>计划状态</summary>
    public MaintenancePlanStatusEnum? Status { get; init; }

    /// <summary>计划日期（起）</summary>
    public DateTime? PlanDateFrom { get; init; }

    /// <summary>计划日期（止）</summary>
    public DateTime? PlanDateTo { get; init; }
}

/// <summary>
/// 保养记录分页查询
/// </summary>
public record MaintenanceRecordPageQuery : PagedRequest
{
    /// <summary>保养记录编号</summary>
    public string? Code { get; init; }

    /// <summary>设备ID</summary>
    public long? EquipmentId { get; init; }

    /// <summary>设备名称</summary>
    public string? EquipmentName { get; init; }

    /// <summary>保养类型</summary>
    public MaintenanceTypeEnum? MaintenanceType { get; init; }

    /// <summary>保养日期（起）</summary>
    public DateTime? MaintenanceDateFrom { get; init; }

    /// <summary>保养日期（止）</summary>
    public DateTime? MaintenanceDateTo { get; init; }
}
