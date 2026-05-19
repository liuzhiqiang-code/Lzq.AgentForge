using FluentValidation;
using FluentValidation.Validators;

namespace Lzq.Rbac.Application.Contracts.Commands;

public record RoleDeleteCommand
{
    public List<long> Ids { get; set; }
    public RoleDeleteCommand(List<long> ids)
    {
        Ids = ids;
    }
}
public class RoleDeleteCommandValidator : MasaAbstractValidator<RoleDeleteCommand>
{
    public RoleDeleteCommandValidator()
    {
        RuleFor(x => x.Ids)
            .NotNull().WithMessage("ID列表不能为null")
            .NotEmpty().WithMessage("ID列表不能为空");

        // 每个ID必须大于0
        RuleForEach(x => x.Ids)
            .GreaterThan(0).WithMessage("ID必须大于0");
    }
}