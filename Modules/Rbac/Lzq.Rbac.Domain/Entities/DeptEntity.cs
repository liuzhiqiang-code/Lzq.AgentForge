using Lzq.Rbac.Domain.Enums;
using Lzq.Extensions.SqlSugar.Entities;
using SqlSugar;

namespace Lzq.Rbac.Domain.Entities;

[Tenant("AgentForge"), SugarTable("rbac_dept")]
public class DeptEntity : BaseFullEntity
{
    /// <summary>
    /// Pid
    /// </summary>
    [SugarColumn(ColumnName = "pid")]
    public long? Pid { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    [SugarColumn(ColumnName = "name", Length = 100)]
    public string Name { get; set; }

    /// <summary>
    /// Status
    /// </summary>
    [SugarColumn(ColumnName = "status")]
    public EnableStatusEnum Status { get; set; }

    /// <summary>
    /// Remark
    /// </summary>
    [SugarColumn(ColumnName = "remark", Length = 2000)]
    public string? Remark { get; set; }

}