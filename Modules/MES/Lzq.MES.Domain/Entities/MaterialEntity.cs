using Lzq.MES.Domain.Enums;
using Lzq.Extensions.SqlSugar.Entities;
using SqlSugar;

namespace Lzq.MES.Domain.Entities;

[Tenant("AgentForge")]
[SugarTable("mes_material")]
public class MaterialEntity : BaseFullEntity
{
    [SugarColumn(ColumnName = "code", Length = 50)]
    public string Code { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "name", Length = 200)]
    public string Name { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "spec", Length = 200, IsNullable = true)]
    public string? Spec { get; set; }

    [SugarColumn(ColumnName = "material_type_id")]
    public long MaterialTypeId { get; set; }

    [SugarColumn(ColumnName = "unit_id")]
    public long UnitId { get; set; }

    [SugarColumn(ColumnName = "weight", IsNullable = true)]
    public decimal? Weight { get; set; }

    [SugarColumn(ColumnName = "volume", IsNullable = true)]
    public decimal? Volume { get; set; }

    [SugarColumn(ColumnName = "status")]
    public MaterialStatusEnum Status { get; set; } = MaterialStatusEnum.Enabled;
}

