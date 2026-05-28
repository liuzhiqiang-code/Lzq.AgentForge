namespace Lzq.MES.Application.Contracts.Dtos;

public record MaterialTypeDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long? ParentId { get; set; }
    public int Level { get; set; }
    public int Sort { get; set; }
    public DateTime CreationTime { get; set; }
}

public record MaterialTypeTreeDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public List<MaterialTypeTreeDto> Children { get; set; } = new();
}

