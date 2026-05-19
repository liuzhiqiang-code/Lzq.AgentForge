using Lzq.Extensions.SqlSugar.Entities;
using Lzq.Rbac.Domain.Enums;
using Lzq.Rbac.Domain.Expands;
using SqlSugar;

namespace Lzq.Rbac.Domain.Entities;

[Tenant("AgentForge"), SugarTable("rbac_menu")]
public class MenuEntity : BaseFullEntity
{
    [SugarColumn(ColumnName = "pid")]
    public long? Pid { get; set; }

    [SugarColumn(ColumnName = "auth_code", Length = 100)]
    public string? AuthCode { get; set; }

    [SugarColumn(ColumnName = "component", Length = 200)]
    public string? Component { get; set; }

    [SugarColumn(ColumnName = "meta", IsJson = true)]
    public MenuMeta? Meta { get; set; }

    [SugarColumn(ColumnName = "name", Length = 100)]
    public string Name { get; set; }

    [SugarColumn(ColumnName = "status")]
    public EnableStatusEnum Status { get; set; } = EnableStatusEnum.Enabled;

    [SugarColumn(ColumnName = "path", Length = 200)]
    public string? Path { get; set; }

    [SugarColumn(ColumnName = "redirect", Length = 100)]
    public string? Redirect { get; set; }

    [SugarColumn(ColumnName = "type", Length = 100)]
    public string Type { get; set; }

    /// <summary>
    /// 子菜单（仅内存使用，不映射数据库）
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public List<MenuEntity>? Children { get; set; }
}
