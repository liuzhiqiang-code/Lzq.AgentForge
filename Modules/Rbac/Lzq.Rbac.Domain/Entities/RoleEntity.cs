using Lzq.Rbac.Domain.Enums;
using Lzq.Extensions.SqlSugar.Entities;
using SqlSugar;

namespace Lzq.Rbac.Domain.Entities;

[Tenant("AgentForge"), SugarTable("rbac_role")]
public class RoleEntity : BaseFullEntity
{
    [SugarColumn(ColumnName = "name", Length = 100)]
    public string Name { get; set; }

    [SugarColumn(ColumnName = "status")]
    public EnableStatusEnum Status { get; set; } = EnableStatusEnum.Enabled;

    [SugarColumn(ColumnName = "remark", Length = 2000)]
    public string? Remark { get; set; }
}
