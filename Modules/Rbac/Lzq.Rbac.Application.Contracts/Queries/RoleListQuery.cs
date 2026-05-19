namespace Lzq.Rbac.Application.Contracts.Queries;

public record RoleListQuery
{
    public long? Id { get; set; }

    public string? Name { get; set; }

    public int? Status { get; set; }

    public string? Remark { get; set; }

    public RoleListQuery()
    {
    }
}