using Lzq.Extensions.SqlSugar.Entities;
using Lzq.Equipment.Domain.Enums;
using SqlSugar;

namespace Lzq.Equipment.Domain.Entities;

/// <summary>
/// 点检计划实体
/// </summary>
[Tenant("AgentForge")]
[SugarTable("mes_inspection_plan")]
public class InspectionPlanEntity : BaseFullEntity
{
    /// <summary>点检计划编号</summary>
    [SugarColumn(ColumnName = "code", Length = 50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>点检计划名称</summary>
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

    /// <summary>点检周期类型：1-每日 2-每周 3-每月 4-自定义</summary>
    [SugarColumn(ColumnName = "cycle_type")]
    public int CycleType { get; set; }

    /// <summary>点检周期值（如：每3天执行一次）</summary>
    [SugarColumn(ColumnName = "cycle_value")]
    public int CycleValue { get; set; } = 1;

    /// <summary>下次点检日期</summary>
    [SugarColumn(ColumnName = "next_inspect_date", IsNullable = true)]
    public DateTime? NextInspectDate { get; set; }

    /// <summary>点检项目数量</summary>
    [SugarColumn(ColumnName = "item_count")]
    public int ItemCount { get; set; }

    /// <summary>执行人ID</summary>
    [SugarColumn(ColumnName = "executor_id", IsNullable = true)]
    public long? ExecutorId { get; set; }

    /// <summary>执行人名称</summary>
    [SugarColumn(ColumnName = "executor_name", Length = 100, IsNullable = true)]
    public string? ExecutorName { get; set; }

    /// <summary>是否启用</summary>
    [SugarColumn(ColumnName = "is_enabled")]
    public bool IsEnabled { get; set; } = true;

    /// <summary>备注</summary>
    [SugarColumn(ColumnName = "remark", Length = 2000, IsNullable = true)]
    public string? Remark { get; set; }
}
