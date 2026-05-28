namespace Lzq.MES.Application.Contracts.ReferenceData;

/// <summary>
/// 产线简单信息（用于跨模块引用）
/// </summary>
public class LineSimpleDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// 工序简单信息（用于跨模块引用）
/// </summary>
public class ProcessSimpleDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
