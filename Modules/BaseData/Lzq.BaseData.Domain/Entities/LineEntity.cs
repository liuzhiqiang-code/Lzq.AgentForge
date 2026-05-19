using Lzq.BaseData.Domain.Enums;
using Lzq.Extensions.SqlSugar.Entities;
using SqlSugar;

namespace Lzq.BaseData.Domain.Entities;

/// <summary>
/// 产线实体
/// </summary>
[Tenant("AgentForge")]
[SugarTable("mes_line")]
public class LineEntity : BaseFullEntity
{
    /// <summary>产线编码</summary>
    [SugarColumn(ColumnName = "code", Length = 50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>产线名称</summary>
    [SugarColumn(ColumnName = "name", Length = 100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>所属车间ID</summary>
    [SugarColumn(ColumnName = "workshop_id")]
    public long WorkshopId { get; set; }

    /// <summary>启用状态</summary>
    [SugarColumn(ColumnName = "status")]
    public EnableStatusEnum Status { get; set; } = EnableStatusEnum.Enabled;
}
