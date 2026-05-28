using Lzq.Core.Models;

namespace Lzq.MES.Application.Contracts.Queries;

public record MaterialPageQuery : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Spec { get; set; }
    public long? MaterialTypeId { get; set; }
    public long? UnitId { get; set; }
    public int? Status { get; set; }
}

