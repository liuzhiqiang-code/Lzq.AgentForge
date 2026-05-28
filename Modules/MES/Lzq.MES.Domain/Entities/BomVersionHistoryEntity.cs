using Lzq.Extensions.SqlSugar.Entities;
using SqlSugar;

namespace Lzq.MES.Domain.Entities;

[Tenant("AgentForge")]
[SugarTable("mes_bom_version_history")]
public class BomVersionHistoryEntity : BaseFullEntity
{
    [SugarColumn(ColumnName = "bom_id")]
    public long BomId { get; set; }

    [SugarColumn(ColumnName = "version", Length = 20)]
    public string Version { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "snapshot_json", ColumnDataType = "nvarchar(max)")]
    public string SnapshotJson { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "change_description", Length = 1000, IsNullable = true)]
    public string? ChangeDescription { get; set; }

    [SugarColumn(ColumnName = "product_id")]
    public long ProductId { get; set; }
}

