using Lzq.Extensions.SqlSugar.Entities;
using Lzq.MES.Domain.Enums;
using SqlSugar;

namespace Lzq.MES.Domain.Entities;

/// <summary>
/// 报修单实体
/// </summary>
[Tenant("AgentForge")]
[SugarTable("mes_repair_order")]
public class RepairOrderEntity : BaseFullEntity
{
    /// <summary>报修单编号</summary>
    [SugarColumn(ColumnName = "code", Length = 50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>关联设备ID</summary>
    [SugarColumn(ColumnName = "equipment_id")]
    public long EquipmentId { get; set; }

    /// <summary>关联设备编号</summary>
    [SugarColumn(ColumnName = "equipment_code", Length = 50, IsNullable = true)]
    public string? EquipmentCode { get; set; }

    /// <summary>关联设备名称</summary>
    [SugarColumn(ColumnName = "equipment_name", Length = 200, IsNullable = true)]
    public string? EquipmentName { get; set; }

    /// <summary>报修类型：1-故障报修 2-点检异常 3-计划维修 4-其他</summary>
    [SugarColumn(ColumnName = "repair_type")]
    public int RepairType { get; set; }

    /// <summary>报修描述</summary>
    [SugarColumn(ColumnName = "description", Length = 2000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>故障图片（JSON数组）</summary>
    [SugarColumn(ColumnName = "images", Length = 4000, IsNullable = true)]
    public string? Images { get; set; }

    /// <summary>报修人ID</summary>
    [SugarColumn(ColumnName = "reporter_id", IsNullable = true)]
    public long? ReporterId { get; set; }

    /// <summary>报修人名称</summary>
    [SugarColumn(ColumnName = "reporter_name", Length = 100, IsNullable = true)]
    public string? ReporterName { get; set; }

    /// <summary>报修时间</summary>
    [SugarColumn(ColumnName = "report_time", IsNullable = true)]
    public DateTime? ReportTime { get; set; }

    /// <summary>优先级</summary>
    [SugarColumn(ColumnName = "priority")]
    public RepairPriorityEnum Priority { get; set; } = RepairPriorityEnum.Medium;

    /// <summary>维修状态</summary>
    [SugarColumn(ColumnName = "status")]
    public RepairStatusEnum Status { get; set; } = RepairStatusEnum.Pending;

    /// <summary>维修人员ID</summary>
    [SugarColumn(ColumnName = "repair_user_id", IsNullable = true)]
    public long? RepairUserId { get; set; }

    /// <summary>维修人员名称</summary>
    [SugarColumn(ColumnName = "repair_user_name", Length = 100, IsNullable = true)]
    public string? RepairUserName { get; set; }

    /// <summary>维修开始时间</summary>
    [SugarColumn(ColumnName = "repair_start_time", IsNullable = true)]
    public DateTime? RepairStartTime { get; set; }

    /// <summary>维修结束时间</summary>
    [SugarColumn(ColumnName = "repair_end_time", IsNullable = true)]
    public DateTime? RepairEndTime { get; set; }

    /// <summary>故障原因</summary>
    [SugarColumn(ColumnName = "fault_reason", Length = 2000, IsNullable = true)]
    public string? FaultReason { get; set; }

    /// <summary>维修处理过程</summary>
    [SugarColumn(ColumnName = "repair_process", Length = 4000, IsNullable = true)]
    public string? RepairProcess { get; set; }

    /// <summary>使用配件（JSON数组）</summary>
    [SugarColumn(ColumnName = "parts_used", Length = 4000, IsNullable = true)]
    public string? PartsUsed { get; set; }

    /// <summary>维修工时（小时）</summary>
    [SugarColumn(ColumnName = "work_hours")]
    public decimal WorkHours { get; set; }

    /// <summary>维修费用</summary>
    [SugarColumn(ColumnName = "cost")]
    public decimal Cost { get; set; }

    /// <summary>验收人ID</summary>
    [SugarColumn(ColumnName = "acceptor_id", IsNullable = true)]
    public long? AcceptorId { get; set; }

    /// <summary>验收人名称</summary>
    [SugarColumn(ColumnName = "acceptor_name", Length = 100, IsNullable = true)]
    public string? AcceptorName { get; set; }

    /// <summary>验收时间</summary>
    [SugarColumn(ColumnName = "accept_time", IsNullable = true)]
    public DateTime? AcceptTime { get; set; }

    /// <summary>验收评价</summary>
    [SugarColumn(ColumnName = "accept_comment", Length = 1000, IsNullable = true)]
    public string? AcceptComment { get; set; }

    /// <summary>备注</summary>
    [SugarColumn(ColumnName = "remark", Length = 2000, IsNullable = true)]
    public string? Remark { get; set; }
}

/// <summary>
/// 保养计划实体
/// </summary>
[Tenant("AgentForge")]
[SugarTable("mes_maintenance_plan")]
public class MaintenancePlanEntity : BaseFullEntity
{
    /// <summary>保养计划编号</summary>
    [SugarColumn(ColumnName = "code", Length = 50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>保养计划名称</summary>
    [SugarColumn(ColumnName = "name", Length = 200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>关联设备ID</summary>
    [SugarColumn(ColumnName = "equipment_id")]
    public long EquipmentId { get; set; }

    /// <summary>关联设备编号</summary>
    [SugarColumn(ColumnName = "equipment_code", Length = 50, IsNullable = true)]
    public string? EquipmentCode { get; set; }

    /// <summary>关联设备名称</summary>
    [SugarColumn(ColumnName = "equipment_name", Length = 200, IsNullable = true)]
    public string? EquipmentName { get; set; }

    /// <summary>保养类型</summary>
    [SugarColumn(ColumnName = "maintenance_type")]
    public MaintenanceTypeEnum MaintenanceType { get; set; }

    /// <summary>保养周期（天）</summary>
    [SugarColumn(ColumnName = "cycle_days")]
    public int CycleDays { get; set; }

    /// <summary>计划保养日期</summary>
    [SugarColumn(ColumnName = "plan_date")]
    public DateTime PlanDate { get; set; }

    /// <summary>实际保养日期</summary>
    [SugarColumn(ColumnName = "actual_date", IsNullable = true)]
    public DateTime? ActualDate { get; set; }

    /// <summary>下次保养日期</summary>
    [SugarColumn(ColumnName = "next_maintenance_date", IsNullable = true)]
    public DateTime? NextMaintenanceDate { get; set; }

    /// <summary>是否启用</summary>
    [SugarColumn(ColumnName = "is_enabled")]
    public bool IsEnabled { get; set; } = true;

    /// <summary>保养计划状态</summary>
    [SugarColumn(ColumnName = "status")]
    public MaintenancePlanStatusEnum Status { get; set; } = MaintenancePlanStatusEnum.Pending;

    /// <summary>负责人ID</summary>
    [SugarColumn(ColumnName = "responsible_id", IsNullable = true)]
    public long? ResponsibleId { get; set; }

    /// <summary>负责人名称</summary>
    [SugarColumn(ColumnName = "responsible_name", Length = 100, IsNullable = true)]
    public string? ResponsibleName { get; set; }

    /// <summary>保养内容</summary>
    [SugarColumn(ColumnName = "content", Length = 2000, IsNullable = true)]
    public string? Content { get; set; }

    /// <summary>保养标准</summary>
    [SugarColumn(ColumnName = "standard", Length = 2000, IsNullable = true)]
    public string? Standard { get; set; }

    /// <summary>保养时长（分钟）</summary>
    [SugarColumn(ColumnName = "duration_minutes")]
    public int DurationMinutes { get; set; }

    /// <summary>备注</summary>
    [SugarColumn(ColumnName = "remark", Length = 2000, IsNullable = true)]
    public string? Remark { get; set; }
}

/// <summary>
/// 保养记录实体
/// </summary>
[Tenant("AgentForge")]
[SugarTable("mes_maintenance_record")]
public class MaintenanceRecordEntity : BaseFullEntity
{
    /// <summary>保养记录编号</summary>
    [SugarColumn(ColumnName = "code", Length = 50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>关联保养计划ID</summary>
    [SugarColumn(ColumnName = "plan_id", IsNullable = true)]
    public long? PlanId { get; set; }

    /// <summary>关联设备ID</summary>
    [SugarColumn(ColumnName = "equipment_id")]
    public long EquipmentId { get; set; }

    /// <summary>关联设备编号</summary>
    [SugarColumn(ColumnName = "equipment_code", Length = 50, IsNullable = true)]
    public string? EquipmentCode { get; set; }

    /// <summary>关联设备名称</summary>
    [SugarColumn(ColumnName = "equipment_name", Length = 200, IsNullable = true)]
    public string? EquipmentName { get; set; }

    /// <summary>保养类型</summary>
    [SugarColumn(ColumnName = "maintenance_type")]
    public MaintenanceTypeEnum MaintenanceType { get; set; }

    /// <summary>保养日期</summary>
    [SugarColumn(ColumnName = "maintenance_date")]
    public DateTime MaintenanceDate { get; set; }

    /// <summary>保养人ID</summary>
    [SugarColumn(ColumnName = "maintainer_id", IsNullable = true)]
    public long? MaintainerId { get; set; }

    /// <summary>保养人名称</summary>
    [SugarColumn(ColumnName = "maintainer_name", Length = 100, IsNullable = true)]
    public string? MaintainerName { get; set; }

    /// <summary>保养时长（分钟）</summary>
    [SugarColumn(ColumnName = "duration_minutes")]
    public int DurationMinutes { get; set; }

    /// <summary>保养内容</summary>
    [SugarColumn(ColumnName = "content", Length = 2000, IsNullable = true)]
    public string? Content { get; set; }

    /// <summary>保养结果</summary>
    [SugarColumn(ColumnName = "result", Length = 1000, IsNullable = true)]
    public string? Result { get; set; }

    /// <summary>保养图片（JSON数组）</summary>
    [SugarColumn(ColumnName = "images", Length = 4000, IsNullable = true)]
    public string? Images { get; set; }

    /// <summary>发现的问题</summary>
    [SugarColumn(ColumnName = "problems_found", Length = 2000, IsNullable = true)]
    public string? ProblemsFound { get; set; }

    /// <summary>下次保养提醒日期</summary>
    [SugarColumn(ColumnName = "next_reminder_date", IsNullable = true)]
    public DateTime? NextReminderDate { get; set; }

    /// <summary>备注</summary>
    [SugarColumn(ColumnName = "remark", Length = 2000, IsNullable = true)]
    public string? Remark { get; set; }
}
