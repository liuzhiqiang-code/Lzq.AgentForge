using Lzq.Extensions.SqlSugar.Entities;
using SqlSugar;

namespace Lzq.MES.Domain.Entities;

[Tenant("AgentForge")]
[SugarTable("mes_material_type")]
public class MaterialTypeEntity : BaseFullEntity
{
    [SugarColumn(ColumnName = "code", Length = 20)]
    public string Code { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "name", Length = 100)]
    public string Name { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "parent_id", IsNullable = true)]
    public long? ParentId { get; set; }

    [SugarColumn(ColumnName = "level")]
    public int Level { get; set; } = 1;

    [SugarColumn(ColumnName = "sort")]
    public int Sort { get; set; } = 0;
}

