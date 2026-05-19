using Lzq.BaseData.Domain.Enums;

namespace Lzq.BaseData.Application.Contracts.Dtos;

public record LineDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long WorkshopId { get; set; }
    public string? WorkshopName { get; set; }
    public EnableStatusEnum Status { get; set; }
    public DateTime CreationTime { get; set; }
}
