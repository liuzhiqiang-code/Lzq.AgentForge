using Lzq.BaseData.Domain.Enums;

namespace Lzq.BaseData.Application.Contracts.Commands;

/// <summary>创建产线命令</summary>
public record LineCreateCommand
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long WorkshopId { get; set; }
    public EnableStatusEnum Status { get; set; } = EnableStatusEnum.Enabled;
}

/// <summary>更新产线命令</summary>
public record LineUpdateCommand
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long WorkshopId { get; set; }
    public EnableStatusEnum Status { get; set; }
}
