namespace Lzq.Temp.Application.Contracts.TestConfig.Commands;

/// <summary>
/// 创建测试配置命令
/// </summary>
public record TestConfigCreateCommand
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Value { get; init; }
    public int ConfigType { get; init; } = 1;
    public bool IsEnabled { get; init; } = true;
    public int SortOrder { get; init; }
    public string? Remark { get; init; }
}
