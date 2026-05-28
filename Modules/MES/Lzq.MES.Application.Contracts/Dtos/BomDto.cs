using Lzq.MES.Domain.Enums;

namespace Lzq.MES.Application.Contracts.Dtos;

public record BomDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public string Version { get; set; } = "V1.0";
    public BomStatusEnum Status { get; set; }
    public DateTime? EffDate { get; set; }
    public DateTime? ExpDate { get; set; }
    public string? Remark { get; set; }
    public DateTime CreationTime { get; set; }
}

public record BomDetailDto
{
    public BomDto Header { get; set; } = null!;
    public List<BomItemDto> Items { get; set; } = new();
}

public record BomItemDto
{
    public long Id { get; set; }
    public long BomId { get; set; }
    public long ItemId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public string? ItemSpec { get; set; }
    public decimal Qty { get; set; }
    public decimal ScrapRate { get; set; }
    public int Sort { get; set; }
    public string? SubstituteIds { get; set; }
    public string? Remark { get; set; }
}

