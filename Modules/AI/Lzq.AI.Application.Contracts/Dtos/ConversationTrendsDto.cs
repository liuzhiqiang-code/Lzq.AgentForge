namespace Lzq.AI.Application.Contracts.Dtos;

public record ConversationTrendsDto
{
    public List<string> Dates { get; set; } = new();
    public int[] Conversations { get; set; } = [];
    public int[] ApiCalls { get; set; } = [];
}