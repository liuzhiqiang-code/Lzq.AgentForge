using Lzq.BaseData.Domain.Enums;

namespace Lzq.BaseData.Application.Contracts.Dtos;

public record ProcessDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long LineId { get; set; }
    public string? LineName { get; set; }
    public int Sequence { get; set; }
    public decimal StandardHours { get; set; }
    public EnableStatusEnum Status { get; set; }
    public DateTime CreationTime { get; set; }
}
