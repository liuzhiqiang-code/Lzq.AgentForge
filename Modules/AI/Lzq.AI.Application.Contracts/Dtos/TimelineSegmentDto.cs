using Lzq.Core.Models;

namespace Lzq.AI.Application.Contracts.Dtos;

public class TimelineSegmentDto
{
    public string Type { get; set; } = string.Empty;     // thinking / message / tool  / echarts
    public string? Content { get; set; }
    public bool? Collapsed { get; set; }
    public string? CallId { get; set; }
    public string? ToolName { get; set; }
    public string? Arguments { get; set; }
    public string? Result { get; set; }
    public string? Status { get; set; }                 // running / done

    public EchartsOption? ChartOption { get; set; }
}
