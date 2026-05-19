using Lzq.Extensions.SqlSugar.Entities;
using SqlSugar;

namespace Lzq.Temp.Domain.Entities.TestConfig;

/// <summary>
/// 测试配置实体
/// </summary>
[Tenant("AgentForge")]
[SugarTable("test_config")]
public class TestConfigEntity : BaseFullEntity
{
    /// <summary>配置编码</summary>
    [SugarColumn(ColumnName = "code", Length = 50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>配置名称</summary>
    [SugarColumn(ColumnName = "name", Length = 200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>配置值</summary>
    [SugarColumn(ColumnName = "value", Length = 500, IsNullable = true)]
    public string? Value { get; set; }

    /// <summary>配置类型：1-系统配置 2-业务配置 3-界面配置</summary>
    [SugarColumn(ColumnName = "config_type")]
    public int ConfigType { get; set; } = 1;

    /// <summary>是否启用</summary>
    [SugarColumn(ColumnName = "is_enabled")]
    public bool IsEnabled { get; set; } = true;

    /// <summary>排序号</summary>
    [SugarColumn(ColumnName = "sort_order")]
    public int SortOrder { get; set; }

    /// <summary>备注</summary>
    [SugarColumn(ColumnName = "remark", Length = 1000, IsNullable = true)]
    public string? Remark { get; set; }
}
