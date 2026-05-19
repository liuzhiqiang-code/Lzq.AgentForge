using Lzq.Equipment.Domain.Enums;

namespace Lzq.Equipment.Application.Contracts.Commands;

/// <summary>
/// 创建报修单命令
/// </summary>
public record RepairOrderCreateCommand
{
    /// <summary>关联设备ID</summary>
    public long EquipmentId { get; init; }

    /// <summary>关联设备编号</summary>
    public string? EquipmentCode { get; init; }

    /// <summary>关联设备名称</summary>
    public string? EquipmentName { get; init; }

    /// <summary>报修类型：1-故障报修 2-点检异常 3-计划维修 4-其他</summary>
    public int RepairType { get; init; }

    /// <summary>报修描述</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>故障图片（JSON数组）</summary>
    public string? Images { get; init; }

    /// <summary>报修人ID</summary>
    public long? ReporterId { get; init; }

    /// <summary>报修人名称</summary>
    public string? ReporterName { get; init; }

    /// <summary>优先级</summary>
    public RepairPriorityEnum Priority { get; init; } = RepairPriorityEnum.Medium;

    /// <summary>备注</summary>
    public string? Remark { get; init; }
}

/// <summary>
/// 派工命令
/// </summary>
public record RepairAssignCommand
{
    /// <summary>报修单ID</summary>
    public long Id { get; init; }

    /// <summary>维修人员ID</summary>
    public long RepairUserId { get; init; }

    /// <summary>维修人员名称</summary>
    public string RepairUserName { get; init; } = string.Empty;
}

/// <summary>
/// 维修开始命令
/// </summary>
public record RepairStartCommand
{
    /// <summary>报修单ID</summary>
    public long Id { get; init; }
}

/// <summary>
/// 维修完成命令
/// </summary>
public record RepairCompleteCommand
{
    /// <summary>报修单ID</summary>
    public long Id { get; init; }

    /// <summary>故障原因</summary>
    public string? FaultReason { get; init; }

    /// <summary>维修处理过程</summary>
    public string? RepairProcess { get; init; }

    /// <summary>使用配件（JSON数组）</summary>
    public string? PartsUsed { get; init; }

    /// <summary>维修工时（小时）</summary>
    public decimal WorkHours { get; init; }

    /// <summary>维修费用</summary>
    public decimal Cost { get; init; }

    /// <summary>备注</summary>
    public string? Remark { get; init; }
}

/// <summary>
/// 验收命令
/// </summary>
public record RepairAcceptCommand
{
    /// <summary>报修单ID</summary>
    public long Id { get; init; }

    /// <summary>验收评价</summary>
    public string? AcceptComment { get; init; }

    /// <summary>备注</summary>
    public string? Remark { get; init; }
}

/// <summary>
/// 创建保养计划命令
/// </summary>
public record MaintenancePlanCreateCommand
{
    /// <summary>保养计划名称</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>关联设备ID</summary>
    public long EquipmentId { get; init; }

    /// <summary>关联设备编号</summary>
    public string? EquipmentCode { get; init; }

    /// <summary>关联设备名称</summary>
    public string? EquipmentName { get; init; }

    /// <summary>保养类型</summary>
    public MaintenanceTypeEnum MaintenanceType { get; init; }

    /// <summary>保养周期（天）</summary>
    public int CycleDays { get; init; }

    /// <summary>计划保养日期</summary>
    public DateTime PlanDate { get; init; }

    /// <summary>负责人ID</summary>
    public long? ResponsibleId { get; init; }

    /// <summary>负责人名称</summary>
    public string? ResponsibleName { get; init; }

    /// <summary>保养内容</summary>
    public string? Content { get; init; }

    /// <summary>保养标准</summary>
    public string? Standard { get; init; }

    /// <summary>保养时长（分钟）</summary>
    public int DurationMinutes { get; init; }

    /// <summary>备注</summary>
    public string? Remark { get; init; }
}

/// <summary>
/// 创建保养记录命令
/// </summary>
public record MaintenanceRecordCreateCommand
{
    /// <summary>关联保养计划ID</summary>
    public long? PlanId { get; init; }

    /// <summary>关联设备ID</summary>
    public long EquipmentId { get; init; }

    /// <summary>关联设备编号</summary>
    public string? EquipmentCode { get; init; }

    /// <summary>关联设备名称</summary>
    public string? EquipmentName { get; init; }

    /// <summary>保养类型</summary>
    public MaintenanceTypeEnum MaintenanceType { get; init; }

    /// <summary>保养日期</summary>
    public DateTime MaintenanceDate { get; init; }

    /// <summary>保养人ID</summary>
    public long? MaintainerId { get; init; }

    /// <summary>保养人名称</summary>
    public string? MaintainerName { get; init; }

    /// <summary>保养时长（分钟）</summary>
    public int DurationMinutes { get; init; }

    /// <summary>保养内容</summary>
    public string? Content { get; init; }

    /// <summary>保养结果</summary>
    public string? Result { get; init; }

    /// <summary>保养图片（JSON数组）</summary>
    public string? Images { get; init; }

    /// <summary>发现的问题</summary>
    public string? ProblemsFound { get; init; }

    /// <summary>备注</summary>
    public string? Remark { get; init; }
}
