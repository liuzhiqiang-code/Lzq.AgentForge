using Lzq.MES.Domain.Enums;
using Lzq.Extensions.SqlSugar.Entities;
using SqlSugar;

namespace Lzq.MES.Domain.Entities;

[Tenant("AgentForge")]
[SugarTable("mes_ecn")]
public class EcnEntity : BaseFullEntity
{
    [SugarColumn(ColumnName = "code", Length = 50)]
    public string Code { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "title", Length = 200)]
    public string Title { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "description", Length = 2000, IsNullable = true)]
    public string? Description { get; set; }

    [SugarColumn(ColumnName = "status")]
    public EcnStatusEnum Status { get; set; } = EcnStatusEnum.Draft;

    [SugarColumn(ColumnName = "reason", Length = 1000, IsNullable = true)]
    public string? Reason { get; set; }

    [SugarColumn(ColumnName = "impact_analysis", ColumnDataType = "nvarchar(max)", IsNullable = true)]
    public string? ImpactAnalysis { get; set; }

    [SugarColumn(ColumnName = "approved_at", IsNullable = true)]
    public DateTime? ApprovedAt { get; set; }

    [SugarColumn(ColumnName = "executed_at", IsNullable = true)]
    public DateTime? ExecutedAt { get; set; }

    [SugarColumn(ColumnName = "confirmed_at", IsNullable = true)]
    public DateTime? ConfirmedAt { get; set; }

    [SugarColumn(ColumnName = "remark", Length = 500, IsNullable = true)]
    public string? Remark { get; set; }
}

