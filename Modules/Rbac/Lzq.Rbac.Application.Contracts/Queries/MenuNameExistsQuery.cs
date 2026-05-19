namespace Lzq.Rbac.Application.Contracts.Queries;

public record MenuNameExistsQuery
{
    public long? Id { get; set; }
    public string? Name { get; set; }

    public MenuNameExistsQuery(long? id, string? name)
    {
        Id = id;
        Name = name;
    }
}
