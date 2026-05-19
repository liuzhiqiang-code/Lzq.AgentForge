namespace Lzq.AI.Application.Contracts.Dtos;

public record ModelDistributionItemDto
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}