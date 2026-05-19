using FluentValidation;
using FluentValidation.Validators;

namespace Lzq.AI.Application.Contracts.Commands;

public record ChatsUpdateTitleCommand
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Title
    /// </summary>
    public string Title { get; set; }

}
public class AIChatsUpdateTitleCommandValidator : MasaAbstractValidator<ChatsUpdateTitleCommand>
{
    public AIChatsUpdateTitleCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("ID必须大于0");

        RuleFor(x => x.Title)
            .NotNull()
            .WithMessage("标题不能为空");
    }
}