using Lzq.MES.Domain.Enums;

namespace Lzq.MES.Application.Contracts.Dtos;

public record UnitOfMeasureDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public UnitCategoryEnum Category { get; set; }
    public DateTime CreationTime { get; set; }
}

