using FluentValidation.Validators;

namespace Lzq.AI.Application.Contracts.Commands;

public record ChatsTitleCommand
{
    public string Instructions { get; set; }

    public string Prompt { get; set; }

    public string Content { get; set; }

    public string Result { get; set; }
}
public class AIChatsTitleCommandValidator : MasaAbstractValidator<ChatsTitleCommand>
{
    public AIChatsTitleCommandValidator()
    {
    }
}