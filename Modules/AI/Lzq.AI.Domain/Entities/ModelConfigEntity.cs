using Lzq.Extensions.SqlSugar.Entities;
using SqlSugar;

namespace Lzq.AI.Domain.Entities;

[Tenant("AgentForge"), SugarTable("ai_model_configs")]
public class ModelConfigEntity : BaseFullEntity
{
    [SugarColumn(ColumnName = "api_key_id")]
    public long ApiKeyId { get; set; }

    [SugarColumn(ColumnName = "config_name")]
    public string ConfigName { get; set; }

    [SugarColumn(ColumnName = "display_model_name")]
    public string DisplayModelName { get; set; }

    [SugarColumn(ColumnName = "context_length")]
    public int? ContextLength { get; set; }

    [SugarColumn(ColumnName = "is_enabled")]
    public bool IsEnabled { get; set; } = true;
}