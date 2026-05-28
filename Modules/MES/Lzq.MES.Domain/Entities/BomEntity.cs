using Lzq.MES.Domain.Enums;
using Lzq.Extensions.SqlSugar.Entities;
using SqlSugar;

namespace Lzq.MES.Domain.Entities;

[Tenant("AgentForge")]
[SugarTable("mes_bom")]
public class BomEntity : BaseFullEntity
{
    [SugarColumn(ColumnName = "code", Length = 50)]
    public string Code { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "name", Length = 200)]
    public string Name { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "product_id")]
    public long ProductId { get; set; }

    [SugarColumn(ColumnName = "version", Length = 20)]
    public string Version { get; set; } = "V1.0";

    [SugarColumn(ColumnName = "status")]
    public BomStatusEnum Status { get; set; } = BomStatusEnum.Draft;

    [SugarColumn(ColumnName = "eff_date", IsNullable = true)]
    public DateTime? EffDate { get; set; }

    [SugarColumn(ColumnName = "exp_date", IsNullable = true)]
    public DateTime? ExpDate { get; set; }

    [SugarColumn(ColumnName = "remark", Length = 500, IsNullable = true)]
    public string? Remark { get; set; }
}

