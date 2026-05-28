namespace Lzq.MES.Application.Contracts.Dtos;

/// <summary>
/// 工厂树形结构DTO（包含车间子节点）
/// </summary>
public record FactoryTreeDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<WorkshopTreeDto> Children { get; set; } = [];
}

/// <summary>
/// 车间树形结构DTO（包含产线子节点）
/// </summary>
public record WorkshopTreeDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long FactoryId { get; set; }
    public string? Manager { get; set; }
    public List<LineTreeDto> Children { get; set; } = [];
}

/// <summary>
/// 产线树形结构DTO（包含工序子节点）
/// </summary>
public record LineTreeDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long WorkshopId { get; set; }
    public List<ProcessTreeDto> Children { get; set; } = [];
}

/// <summary>
/// 工序树形结构DTO（叶子节点）
/// </summary>
public record ProcessTreeDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public decimal StandardHours { get; set; }
}
