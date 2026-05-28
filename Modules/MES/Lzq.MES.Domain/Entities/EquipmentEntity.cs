using Lzq.Extensions.SqlSugar.Entities;
using Lzq.MES.Domain.Enums;
using SqlSugar;

namespace Lzq.MES.Domain.Entities;

/// <summary>
/// 设备台账实体
/// </summary>
[Tenant("AgentForge")]
[SugarTable("mes_equipment")]
public class EquipmentEntity : BaseFullEntity
{
    /// <summary>设备编号</summary>
    [SugarColumn(ColumnName = "code", Length = 50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>设备名称</summary>
    [SugarColumn(ColumnName = "name", Length = 200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>设备类型</summary>
    [SugarColumn(ColumnName = "equipment_type")]
    public EquipmentTypeEnum EquipmentType { get; set; }

    /// <summary>规格型号</summary>
    [SugarColumn(ColumnName = "spec", Length = 200, IsNullable = true)]
    public string? Spec { get; set; }

    /// <summary>品牌/厂商</summary>
    [SugarColumn(ColumnName = "brand", Length = 200, IsNullable = true)]
    public string? Brand { get; set; }

    /// <summary>供应商</summary>
    [SugarColumn(ColumnName = "supplier", Length = 200, IsNullable = true)]
    public string? Supplier { get; set; }

    /// <summary>购买日期</summary>
    [SugarColumn(ColumnName = "purchase_date", IsNullable = true)]
    public DateTime? PurchaseDate { get; set; }

    /// <summary>保修截止日期</summary>
    [SugarColumn(ColumnName = "warranty_end_date", IsNullable = true)]
    public DateTime? WarrantyEndDate { get; set; }

    /// <summary>设备状态</summary>
    [SugarColumn(ColumnName = "status")]
    public EquipmentStatusEnum Status { get; set; } = EquipmentStatusEnum.Normal;

    /// <summary>所属产线ID</summary>
    [SugarColumn(ColumnName = "line_id", IsNullable = true)]
    public long? LineId { get; set; }

    /// <summary>所属产线名称</summary>
    [SugarColumn(ColumnName = "line_name", Length = 200, IsNullable = true)]
    public string? LineName { get; set; }

    /// <summary>安装位置</summary>
    [SugarColumn(ColumnName = "location", Length = 200, IsNullable = true)]
    public string? Location { get; set; }

    /// <summary>责任人ID</summary>
    [SugarColumn(ColumnName = "responsible_id", IsNullable = true)]
    public long? ResponsibleId { get; set; }

    /// <summary>责任人名称</summary>
    [SugarColumn(ColumnName = "responsible_name", Length = 100, IsNullable = true)]
    public string? ResponsibleName { get; set; }

    /// <summary>设备照片（JSON数组）</summary>
    [SugarColumn(ColumnName = "photos", Length = 4000, IsNullable = true)]
    public string? Photos { get; set; }

    /// <summary>设备参数（JSON）</summary>
    [SugarColumn(ColumnName = "parameters", Length = 4000, IsNullable = true)]
    public string? Parameters { get; set; }

    /// <summary>累计运行时间（小时）</summary>
    [SugarColumn(ColumnName = "total_running_hours")]
    public decimal TotalRunningHours { get; set; }

    /// <summary>累计维修次数</summary>
    [SugarColumn(ColumnName = "total_repair_count")]
    public int TotalRepairCount { get; set; }

    /// <summary>备注</summary>
    [SugarColumn(ColumnName = "remark", Length = 2000, IsNullable = true)]
    public string? Remark { get; set; }
}
