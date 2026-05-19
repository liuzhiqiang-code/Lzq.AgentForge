using FluentValidation;
using FluentValidation.Validators;
using Lzq.Rbac.Domain.Enums;

namespace Lzq.Rbac.Application.Contracts.Commands;
public record RoleUpdateCommand
{
    public long Id { get; set; }
    public string Name { get; set; }
    public EnableStatusEnum Status { get; set; } = EnableStatusEnum.Enabled;
    public string? Remark { get; set; }
    public List<long> Permissions { get; set; } = [];
}
public class RoleUpdateCommandValidator : MasaAbstractValidator<RoleUpdateCommand>
{
    public RoleUpdateCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("ID必须大于0");

        RuleFor(a => a.Name)
            .NotNull()
            .NotEmpty()
            .WithMessage("角色名称不能为空");

        WhenNotEmpty(a => a.Remark,
            rule => rule
            .Length(0, 500)
            .WithMessage("备注信息不能超过500字符"));
    }
}