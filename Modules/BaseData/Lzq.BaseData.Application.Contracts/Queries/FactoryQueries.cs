using Lzq.Core.Models;

namespace Lzq.BaseData.Application.Contracts.Queries;

/// <summary>工厂分页查询</summary>
public record FactoryPageQuery : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public int? Status { get; set; }
}
