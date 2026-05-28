using Lzq.Core.Models;

namespace Lzq.MES.Application.Contracts.Queries;

public record BomPageQuery : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public long? ProductId { get; set; }
    public int? Status { get; set; }
}

