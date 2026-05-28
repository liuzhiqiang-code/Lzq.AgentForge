namespace Lzq.MES.Application.Contracts.Dtos;

public record BomSnapshot
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long ProductId { get; set; }
    public string Version { get; set; } = string.Empty;
    public DateTime? EffDate { get; set; }
    public DateTime? ExpDate { get; set; }
    public string? Remark { get; set; }
    public List<BomItemSnapshot> Items { get; set; } = new();
}

public record BomItemSnapshot
{
    public long ItemId { get; set; }
    public decimal Qty { get; set; }
    public decimal ScrapRate { get; set; }
    public int Sort { get; set; }
    public string? SubstituteIds { get; set; }
    public string? Remark { get; set; }
}

