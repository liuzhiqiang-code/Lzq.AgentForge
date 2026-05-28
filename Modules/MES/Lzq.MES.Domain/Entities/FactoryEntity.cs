using Lzq.MES.Domain.Enums;
using Lzq.Extensions.SqlSugar.Entities;
using SqlSugar;

namespace Lzq.MES.Domain.Entities;

/// <summary>
/// 工厂实体
/// </summary>
[Tenant("AgentForge")]
[SugarTable("mes_factory")]
public class FactoryEntity : BaseFullEntity
{
    /// <summary>工厂编码</summary>
    [SugarColumn(ColumnName = "code", Length = 50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>工厂名称</summary>
    [SugarColumn(ColumnName = "name", Length = 100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>工厂地址</summary>
    [SugarColumn(ColumnName = "address", Length = 500, IsNullable = true)]
    public string? Address { get; set; }

    /// <summary>启用状态</summary>
    [SugarColumn(ColumnName = "status")]
    public EnableStatusEnum Status { get; set; } = EnableStatusEnum.Enabled;
}
