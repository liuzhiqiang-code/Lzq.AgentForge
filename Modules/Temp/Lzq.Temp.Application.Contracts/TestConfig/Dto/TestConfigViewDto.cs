namespace Lzq.Temp.Application.Contracts.TestConfig.Dto;

/// <summary>
/// 测试配置视图 DTO
/// </summary>
public class TestConfigViewDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Value { get; set; }
    public int ConfigType { get; set; }
    public bool IsEnabled { get; set; }
    public int SortOrder { get; set; }
    public string? Remark { get; set; }
    public DateTime CreateTime { get; set; }
}
