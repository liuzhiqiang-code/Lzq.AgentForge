using Lzq.MES.Domain.Enums;
using Lzq.Extensions.SqlSugar.Entities;
using SqlSugar;

namespace Lzq.MES.Domain.Entities;

[Tenant("AgentForge")]
[SugarTable("mes_ecn_item")]
public class EcnItemEntity : BaseFullEntity
{
    [SugarColumn(ColumnName = "ecn_id")]
    public long EcnId { get; set; }

    [SugarColumn(ColumnName = "change_type")]
    public EcnChangeTypeEnum ChangeType { get; set; }

    [SugarColumn(ColumnName = "target_id")]
    public long TargetId { get; set; }

    [SugarColumn(ColumnName = "target_code", Length = 50, IsNullable = true)]
    public string? TargetCode { get; set; }

    [SugarColumn(ColumnName = "target_name", Length = 200, IsNullable = true)]
    public string? TargetName { get; set; }

    [SugarColumn(ColumnName = "change_summary", Length = 500, IsNullable = true)]
    public string? ChangeSummary { get; set; }

    [SugarColumn(ColumnName = "before_snapshot", ColumnDataType = "nvarchar(max)", IsNullable = true)]
    public string? BeforeSnapshot { get; set; }

    [SugarColumn(ColumnName = "after_snapshot", ColumnDataType = "nvarchar(max)", IsNullable = true)]
    public string? AfterSnapshot { get; set; }

    [SugarColumn(ColumnName = "remark", Length = 500, IsNullable = true)]
    public string? Remark { get; set; }
}

