using Lzq.MES.Domain.Enums;
using Lzq.Extensions.SqlSugar.Entities;
using SqlSugar;

namespace Lzq.MES.Domain.Entities;

/// <summary>
/// 工序实体
/// </summary>
[Tenant("AgentForge")]
[SugarTable("mes_process")]
public class ProcessEntity : BaseFullEntity
{
    /// <summary>工序编码</summary>
    [SugarColumn(ColumnName = "code", Length = 50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>工序名称</summary>
    [SugarColumn(ColumnName = "name", Length = 100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>所属产线ID</summary>
    [SugarColumn(ColumnName = "line_id")]
    public long LineId { get; set; }

    /// <summary>工序顺序</summary>
    [SugarColumn(ColumnName = "sequence")]
    public int Sequence { get; set; }

    /// <summary>标准工时（分钟）</summary>
    [SugarColumn(ColumnName = "standard_hours")]
    public decimal StandardHours { get; set; }

    /// <summary>启用状态</summary>
    [SugarColumn(ColumnName = "status")]
    public EnableStatusEnum Status { get; set; } = EnableStatusEnum.Enabled;
}
