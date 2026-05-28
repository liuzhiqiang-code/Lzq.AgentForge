using Lzq.Extensions.SqlSugar.Entities;
using SqlSugar;

namespace Lzq.MES.Domain.Entities;

/// <summary>
/// 点检明细实体
/// </summary>
[Tenant("AgentForge")]
[SugarTable("mes_inspection_item")]
public class InspectionItemEntity : BaseFullEntity
{
    /// <summary>点检计划ID</summary>
    [SugarColumn(ColumnName = "plan_id")]
    public long PlanId { get; set; }

    /// <summary>点检项目名称</summary>
    [SugarColumn(ColumnName = "item_name", Length = 200)]
    public string ItemName { get; set; } = string.Empty;

    /// <summary>点检标准/要求</summary>
    [SugarColumn(ColumnName = "standard", Length = 500, IsNullable = true)]
    public string? Standard { get; set; }

    /// <summary>点检方法</summary>
    [SugarColumn(ColumnName = "method", Length = 500, IsNullable = true)]
    public string? Method { get; set; }

    /// <summary>项目类型：1-设备状态 2-运行参数 3-安全检查 4-清洁度 5-润滑 6-其他</summary>
    [SugarColumn(ColumnName = "item_type")]
    public int ItemType { get; set; }

    /// <summary>是否必检项</summary>
    [SugarColumn(ColumnName = "is_required")]
    public bool IsRequired { get; set; } = true;

    /// <summary>排序号</summary>
    [SugarColumn(ColumnName = "sort")]
    public int Sort { get; set; }

    /// <summary>备注</summary>
    [SugarColumn(ColumnName = "remark", Length = 500, IsNullable = true)]
    public string? Remark { get; set; }
}
