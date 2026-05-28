using Lzq.Core.Models;

namespace Lzq.MES.Application.Contracts.Queries;

public record MaterialTypePageQuery : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public long? ParentId { get; set; }
}

