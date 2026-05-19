namespace Lzq.AI.Application.Contracts.Dtos;

public record SkillStatsItemDto
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}
