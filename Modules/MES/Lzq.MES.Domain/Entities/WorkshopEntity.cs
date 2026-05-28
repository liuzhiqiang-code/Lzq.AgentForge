using Lzq.MES.Domain.Enums;
using Lzq.Extensions.SqlSugar.Entities;
using SqlSugar;

namespace Lzq.MES.Domain.Entities;

/// <summary>
/// 车间实体
/// </summary>
[Tenant("AgentForge")]
[SugarTable("mes_workshop")]
public class WorkshopEntity : BaseFullEntity
{
    /// <summary>车间编码</summary>
    [SugarColumn(ColumnName = "code", Length = 50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>车间名称</summary>
    [SugarColumn(ColumnName = "name", Length = 100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>所属工厂ID</summary>
    [SugarColumn(ColumnName = "factory_id")]
    public long FactoryId { get; set; }

    /// <summary>车间负责人</summary>
    [SugarColumn(ColumnName = "manager", Length = 50, IsNullable = true)]
    public string? Manager { get; set; }

    /// <summary>启用状态</summary>
    [SugarColumn(ColumnName = "status")]
    public EnableStatusEnum Status { get; set; } = EnableStatusEnum.Enabled;
}
