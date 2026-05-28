namespace Lzq.MES.Application.Contracts.Commands;

public record MaterialTypeCreateCommand
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long? ParentId { get; set; }
    public int Sort { get; set; } = 0;
}

public record MaterialTypeUpdateCommand
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long? ParentId { get; set; }
    public int Sort { get; set; }
}

