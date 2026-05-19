namespace Lzq.Rbac.Application.Contracts.Queries;

public record MenuPathExistsQuery
{
    public long? Id { get; set; }
    public string? Path { get; set; }

    public MenuPathExistsQuery(long? id, string? path)
    {
        Id = id;
        Path = path;
    }
}
