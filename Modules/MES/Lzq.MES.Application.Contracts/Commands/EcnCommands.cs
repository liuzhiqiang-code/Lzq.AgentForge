using Lzq.MES.Domain.Enums;

namespace Lzq.MES.Application.Contracts.Commands;

public record EcnCreateCommand
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Reason { get; set; }
    public string? Remark { get; set; }
}

public record EcnUpdateCommand
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Reason { get; set; }
    public string? Remark { get; set; }
}

public record EcnItemCreateCommand
{
    public long EcnId { get; set; }
    public EcnChangeTypeEnum ChangeType { get; set; }
    public long TargetId { get; set; }
    public string? TargetCode { get; set; }
    public string? TargetName { get; set; }
    public string? ChangeSummary { get; set; }
    public string? BeforeSnapshot { get; set; }
    public string? AfterSnapshot { get; set; }
    public string? Remark { get; set; }
}

public record EcnSubmitCommand
{
    public long Id { get; set; }
}

public record EcnApproveCommand
{
    public long Id { get; set; }
}

public record EcnExecuteCommand
{
    public long Id { get; set; }
    public List<EcnItemExecuteDto> Items { get; set; } = new();
}

public record EcnItemExecuteDto
{
    public long EcnItemId { get; set; }
    public string? AfterSnapshot { get; set; }
}

public record EcnConfirmCommand
{
    public long Id { get; set; }
}

public record EcnCancelCommand
{
    public long Id { get; set; }
}

