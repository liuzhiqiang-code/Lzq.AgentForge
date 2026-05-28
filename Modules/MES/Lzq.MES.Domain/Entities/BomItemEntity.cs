using Lzq.Extensions.SqlSugar.Entities;
using SqlSugar;

namespace Lzq.MES.Domain.Entities;

[Tenant("AgentForge")]
[SugarTable("mes_bom_item")]
public class BomItemEntity : BaseFullEntity
{
    [SugarColumn(ColumnName = "bom_id")]
    public long BomId { get; set; }

    [SugarColumn(ColumnName = "item_id")]
    public long ItemId { get; set; }

    [SugarColumn(ColumnName = "qty")]
    public decimal Qty { get; set; } = 1;

    [SugarColumn(ColumnName = "scrap_rate")]
    public decimal ScrapRate { get; set; } = 0;

    [SugarColumn(ColumnName = "sort")]
    public int Sort { get; set; } = 0;

    [SugarColumn(ColumnName = "substitute_ids", Length = 500, IsNullable = true)]
    public string? SubstituteIds { get; set; }

    [SugarColumn(ColumnName = "remark", Length = 500, IsNullable = true)]
    public string? Remark { get; set; }
}

