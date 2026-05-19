using Lzq.Extensions.SqlSugar.Entities;
using SqlSugar;

namespace Lzq.Dashboard.Domain.Entities;

/// <summary>
/// 看板配置实体
/// </summary>
[Tenant("AgentForge")]
[SugarTable("dashboard_config")]
public class DashboardConfigEntity : BaseFullEntity
{
    /// <summary>配置编码</summary>
    [SugarColumn(ColumnName = "code", Length = 50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>配置名称</summary>
    [SugarColumn(ColumnName = "name", Length = 100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>配置类型：1-看板配置，2-报表配置</summary>
    [SugarColumn(ColumnName = "config_type")]
    public int ConfigType { get; set; } = 1;

    /// <summary>刷新间隔（秒）</summary>
    [SugarColumn(ColumnName = "refresh_interval")]
    public int RefreshInterval { get; set; } = 60;

    /// <summary>缓存时间（秒）</summary>
    [SugarColumn(ColumnName = "cache_ttl")]
    public int CacheTtl { get; set; } = 300;

    /// <summary>配置JSON</summary>
    [SugarColumn(ColumnName = "config_json", Length = 4000, IsNullable = true)]
    public string? ConfigJson { get; set; }

    /// <summary>是否启用</summary>
    [SugarColumn(ColumnName = "is_enabled")]
    public bool IsEnabled { get; set; } = true;

    /// <summary>备注</summary>
    [SugarColumn(ColumnName = "remark", Length = 2000, IsNullable = true)]
    public string? Remark { get; set; }
}
