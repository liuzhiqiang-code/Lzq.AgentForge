using FluentValidation;
using FluentValidation.Validators;
using Lzq.Rbac.Domain.Enums;

namespace Lzq.Rbac.Application.Contracts.Commands;

public record DeptCreateCommand
{
    public long? Pid { get; set; }
    public string Name { get; set; }
    public EnableStatusEnum Status { get; set; } = EnableStatusEnum.Enabled;
    public string? Remark { get; set; }
}
public class DeptCreateCommandValidator : MasaAbstractValidator<DeptCreateCommand>
{
    public DeptCreateCommandValidator()
    {
        RuleFor(a => a.Name)
            .NotNull()
            .NotEmpty()
            .WithMessage("部门名称不能为空");

        WhenNotEmpty(a => a.Remark,
            rule => rule
            .Length(0, 500)
            .WithMessage("备注信息不能超过500字符"));
    }
}