using Lzq.Core.Models;

namespace Lzq.MES.Application.Contracts.Queries;

public record UnitOfMeasurePageQuery : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public int? Category { get; set; }
}

