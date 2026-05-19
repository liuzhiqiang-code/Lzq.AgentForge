using FluentValidation;
using FluentValidation.Validators;

/*
 * @author : liuzhiqiang
 * @date : 2026-1-1
 * @desc : user
 */
namespace Lzq.Rbac.Application.Contracts.Commands;

public record UserDeleteCommand
{
    public List<long> Ids { get; set; }
    public UserDeleteCommand(List<long> ids)
    {
        Ids = ids;
    }
}
public class UserDeleteCommandValidator : MasaAbstractValidator<UserDeleteCommand>
{
    public UserDeleteCommandValidator()
    {
        RuleFor(x => x.Ids)
            .NotNull().WithMessage("ID列表不能为null")
            .NotEmpty().WithMessage("ID列表不能为空");
        // 每个ID必须大于0
        RuleForEach(x => x.Ids)
            .GreaterThan(0).WithMessage("ID必须大于0");
    }
}