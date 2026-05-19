using FluentValidation;
using FluentValidation.Validators;

namespace Lzq.AI.Application.Contracts.Commands;

public record ModelConfigDeleteCommand
{
    public List<long> Ids { get; set; }
    public ModelConfigDeleteCommand(List<long> ids)
    {
        Ids = ids;
    }
}
public class AIModelConfigDeleteCommandValidator : MasaAbstractValidator<ModelConfigDeleteCommand>
{
    public AIModelConfigDeleteCommandValidator()
    {
        RuleFor(x => x.Ids)
            .NotNull().WithMessage("ID列表不能为null")
            .NotEmpty().WithMessage("ID列表不能为空");
        // 每个ID必须大于0
        RuleForEach(x => x.Ids)
            .GreaterThan(0).WithMessage("ID必须大于0");
    }
}