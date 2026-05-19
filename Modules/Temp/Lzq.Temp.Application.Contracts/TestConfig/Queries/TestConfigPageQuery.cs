using Lzq.Core.Models;

namespace Lzq.Temp.Application.Contracts.TestConfig.Queries;

/// <summary>
/// 测试配置分页查询
/// </summary>
public record TestConfigPageQuery : PagedRequest
{
    public string? Keyword { get; init; }

    public int? ConfigType { get; init; }
}
