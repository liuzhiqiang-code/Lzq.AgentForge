namespace Lzq.AI.Application.Contracts.Dtos;

public record ModelUsageMonthlyDto
{
    public string[] Months { get; set; } = [];
    public int[] Values { get; set; } = [];
}