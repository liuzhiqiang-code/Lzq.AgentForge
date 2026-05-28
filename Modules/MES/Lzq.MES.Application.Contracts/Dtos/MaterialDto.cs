using Lzq.MES.Domain.Enums;

namespace Lzq.MES.Application.Contracts.Dtos;

public record MaterialDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Spec { get; set; }
    public long MaterialTypeId { get; set; }
    public string? MaterialTypeName { get; set; }
    public long UnitId { get; set; }
    public string? UnitName { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Volume { get; set; }
    public MaterialStatusEnum Status { get; set; }
    public DateTime CreationTime { get; set; }
}

