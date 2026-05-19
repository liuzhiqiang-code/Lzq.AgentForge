using Lzq.BaseData.Domain.Enums;

namespace Lzq.BaseData.Application.Contracts.Commands;

/// <summary>创建车间命令</summary>
public record WorkshopCreateCommand
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long FactoryId { get; set; }
    public string? Manager { get; set; }
    public EnableStatusEnum Status { get; set; } = EnableStatusEnum.Enabled;
}

/// <summary>更新车间命令</summary>
public record WorkshopUpdateCommand
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long FactoryId { get; set; }
    public string? Manager { get; set; }
    public EnableStatusEnum Status { get; set; }
}
