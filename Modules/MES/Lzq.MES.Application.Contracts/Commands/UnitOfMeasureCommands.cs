using Lzq.MES.Domain.Enums;

namespace Lzq.MES.Application.Contracts.Commands;

public record UnitOfMeasureCreateCommand
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public UnitCategoryEnum Category { get; set; } = UnitCategoryEnum.Count;
}

public record UnitOfMeasureUpdateCommand
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public UnitCategoryEnum Category { get; set; }
}

