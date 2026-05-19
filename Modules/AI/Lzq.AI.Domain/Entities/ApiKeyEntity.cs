using Lzq.AI.Domain.Enums;
using Lzq.Extensions.SqlSugar.Entities;
using SqlSugar;

namespace Lzq.AI.Domain.Entities;

[Tenant("AgentForge"), SugarTable("ai_api_key")]
public class ApiKeyEntity : BaseFullEntity
{
    /// <summary>
    /// 厂商名称
    /// </summary>
    [SugarColumn(ColumnName = "provider", ColumnDescription = "厂商名称")]
    public ProviderEnum Provider { get; set; }

    /// <summary>
    /// key名称
    /// </summary>
    [SugarColumn(ColumnName = "key_name", ColumnDescription = "key名称")]
    public string KeyName { get; set; }

    /// <summary>
    /// 加密后的key值
    /// </summary>
    [SugarColumn(ColumnName = "key_value", ColumnDescription = "加密后的key值")]
    public string KeyValue { get; set; }

    /// <summary>
    /// 加密key的iv值
    /// </summary>
    [SugarColumn(ColumnName = "key_iv", ColumnDescription = "加密key的iv值")]
    public string KeyIv { get; set; }

    /// <summary>
    /// base_url
    /// </summary>
    [SugarColumn(ColumnName = "base_url", ColumnDescription = "base_url")]
    public string? BaseUrl { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [SugarColumn(ColumnName = "is_enabled", ColumnDescription = "是否启用")]
    public bool IsEnabled { get; set; } = false;
}