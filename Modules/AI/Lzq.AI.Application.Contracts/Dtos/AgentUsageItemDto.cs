namespace Lzq.AI.Application.Contracts.Dtos;

public record AgentUsageItemDto
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}