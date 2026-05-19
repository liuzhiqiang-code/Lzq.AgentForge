using Lzq.Core.Models;

namespace Lzq.Rbac.Application.Contracts.Queries;

public record RolePageQuery : PagedRequest
{
    public long? Id { get; set; }

    public string? Name { get; set; }

    public int? Status { get; set; }

    public string? Remark { get; set; }

    public RolePageQuery()
    {
    }
}
