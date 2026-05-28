using Lzq.MES.Domain.Enums;

namespace Lzq.MES.Application.Contracts.Dtos;

public record WorkshopDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long FactoryId { get; set; }
    public string? FactoryName { get; set; }
    public string? Manager { get; set; }
    public EnableStatusEnum Status { get; set; }
    public DateTime CreationTime { get; set; }
}
