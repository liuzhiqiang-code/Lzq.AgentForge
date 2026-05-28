using Lzq.MES.Domain.Enums;

namespace Lzq.MES.Application.Contracts.Commands;

/// <summary>创建工序命令</summary>
public record ProcessCreateCommand
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long LineId { get; set; }
    public int Sequence { get; set; }
    public decimal StandardHours { get; set; }
    public EnableStatusEnum Status { get; set; } = EnableStatusEnum.Enabled;
}

/// <summary>更新工序命令</summary>
public record ProcessUpdateCommand
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long LineId { get; set; }
    public int Sequence { get; set; }
    public decimal StandardHours { get; set; }
    public EnableStatusEnum Status { get; set; }
}
