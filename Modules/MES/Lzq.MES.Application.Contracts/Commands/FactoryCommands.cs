using Lzq.MES.Domain.Enums;

namespace Lzq.MES.Application.Contracts.Commands;

/// <summary>创建工厂命令</summary>
public record FactoryCreateCommand
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public EnableStatusEnum Status { get; set; } = EnableStatusEnum.Enabled;
}

/// <summary>更新工厂命令</summary>
public record FactoryUpdateCommand
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public EnableStatusEnum Status { get; set; }
}
