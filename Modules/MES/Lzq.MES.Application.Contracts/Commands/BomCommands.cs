using Lzq.MES.Domain.Enums;

namespace Lzq.MES.Application.Contracts.Commands;

public record BomCreateCommand
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long ProductId { get; set; }
    public string Version { get; set; } = "V1.0";
    public DateTime? EffDate { get; set; }
    public DateTime? ExpDate { get; set; }
    public string? Remark { get; set; }
}

public record BomUpdateCommand
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long ProductId { get; set; }
    public string Version { get; set; } = "V1.0";
    public DateTime? EffDate { get; set; }
    public DateTime? ExpDate { get; set; }
    public string? Remark { get; set; }
}

public record BomItemCreateCommand
{
    public long BomId { get; set; }
    public long ItemId { get; set; }
    public decimal Qty { get; set; } = 1;
    public decimal ScrapRate { get; set; }
    public int Sort { get; set; }
    public string? SubstituteIds { get; set; }
    public string? Remark { get; set; }
}

public record BomItemUpdateCommand
{
    public long Id { get; set; }
    public long ItemId { get; set; }
    public decimal Qty { get; set; } = 1;
    public decimal ScrapRate { get; set; }
    public int Sort { get; set; }
    public string? SubstituteIds { get; set; }
    public string? Remark { get; set; }
}

