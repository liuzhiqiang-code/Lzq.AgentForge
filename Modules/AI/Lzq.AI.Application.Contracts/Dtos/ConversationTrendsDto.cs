namespace Lzq.AI.Application.Contracts.Dtos;

public record ConversationTrendsDto
{
    public List<string> Dates { get; set; } = new();
    public int[] UserRequests { get; set; } = [];
    public int[] AssistantResponses { get; set; } = [];
}