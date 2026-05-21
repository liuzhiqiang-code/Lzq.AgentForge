namespace Lzq.AI.Application.Contracts.Dtos;

public class ChatHistoryViewDto
{
    public long Id { get; set; }

    public string Role { get; set; }

    public string Content { get; set; }

    public List<TimelineSegmentDto>? Segments { get; set; }

}
