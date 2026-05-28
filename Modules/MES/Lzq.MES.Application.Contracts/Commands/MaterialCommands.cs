using Lzq.MES.Domain.Enums;

namespace Lzq.MES.Application.Contracts.Commands;

public record MaterialCreateCommand
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Spec { get; set; }
    public long MaterialTypeId { get; set; }
    public long UnitId { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Volume { get; set; }
    public MaterialStatusEnum Status { get; set; } = MaterialStatusEnum.Enabled;
}

public record MaterialUpdateCommand
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Spec { get; set; }
    public long MaterialTypeId { get; set; }
    public long UnitId { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Volume { get; set; }
    public MaterialStatusEnum Status { get; set; }
}

