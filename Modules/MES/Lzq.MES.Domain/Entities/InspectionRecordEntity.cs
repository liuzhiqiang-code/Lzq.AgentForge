using Lzq.Extensions.SqlSugar.Entities;
using Lzq.MES.Domain.Enums;
using SqlSugar;

namespace Lzq.MES.Domain.Entities;

/// <summary>
/// 点检记录实体
/// </summary>
[Tenant("AgentForge")]
[SugarTable("mes_inspection_record")]
public class InspectionRecordEntity : BaseFullEntity
{
    /// <summary>点检记录编号</summary>
    [SugarColumn(ColumnName = "code", Length = 50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>关联点检计划ID</summary>
    [SugarColumn(ColumnName = "plan_id")]
    public long PlanId { get; set; }

    /// <summary>关联设备ID</summary>
    [SugarColumn(ColumnName = "equipment_id")]
    public long EquipmentId { get; set; }

    /// <summary>关联设备编号</summary>
    [SugarColumn(ColumnName = "equipment_code", Length = 50, IsNullable = true)]
    public string? EquipmentCode { get; set; }

    /// <summary>关联设备名称</summary>
    [SugarColumn(ColumnName = "equipment_name", Length = 200, IsNullable = true)]
    public string? EquipmentName { get; set; }

    /// <summary>点检日期</summary>
    [SugarColumn(ColumnName = "inspect_date")]
    public DateTime InspectDate { get; set; }

    /// <summary>点检结果</summary>
    [SugarColumn(ColumnName = "result")]
    public InspectionResultEnum Result { get; set; } = InspectionResultEnum.Normal;

    /// <summary>执行人ID</summary>
    [SugarColumn(ColumnName = "inspector_id", IsNullable = true)]
    public long? InspectorId { get; set; }

    /// <summary>执行人名称</summary>
    [SugarColumn(ColumnName = "inspector_name", Length = 100, IsNullable = true)]
    public string? InspectorName { get; set; }

    /// <summary>完成时间</summary>
    [SugarColumn(ColumnName = "completed_time", IsNullable = true)]
    public DateTime? CompletedTime { get; set; }

    /// <summary>点检时长（分钟）</summary>
    [SugarColumn(ColumnName = "duration_minutes")]
    public int DurationMinutes { get; set; }

    /// <summary>异常描述</summary>
    [SugarColumn(ColumnName = "abnormal_desc", Length = 2000, IsNullable = true)]
    public string? AbnormalDesc { get; set; }

    /// <summary>是否生成报修单</summary>
    [SugarColumn(ColumnName = "create_repair_order")]
    public bool CreateRepairOrder { get; set; }

    /// <summary>关联报修单ID</summary>
    [SugarColumn(ColumnName = "repair_order_id", IsNullable = true)]
    public long? RepairOrderId { get; set; }

    /// <summary>备注</summary>
    [SugarColumn(ColumnName = "remark", Length = 2000, IsNullable = true)]
    public string? Remark { get; set; }
}
