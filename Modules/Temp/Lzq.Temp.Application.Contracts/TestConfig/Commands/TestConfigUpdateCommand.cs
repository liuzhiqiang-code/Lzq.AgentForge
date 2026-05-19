namespace Lzq.Temp.Application.Contracts.TestConfig.Commands;

/// <summary>
/// 更新测试配置命令
/// </summary>
public record TestConfigUpdateCommand
{
    public long Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Value { get; init; }
    public int ConfigType { get; init; }
    public bool IsEnabled { get; init; }
    public int SortOrder { get; init; }
    public string? Remark { get; init; }
}
