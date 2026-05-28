using Lzq.MES.Domain.Enums;
using Lzq.Extensions.SqlSugar.Entities;
using SqlSugar;

namespace Lzq.MES.Domain.Entities;

[Tenant("AgentForge")]
[SugarTable("mes_unit_of_measure")]
public class UnitOfMeasureEntity : BaseFullEntity
{
    [SugarColumn(ColumnName = "code", Length = 20)]
    public string Code { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "name", Length = 50)]
    public string Name { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "category")]
    public UnitCategoryEnum Category { get; set; } = UnitCategoryEnum.Count;
}

