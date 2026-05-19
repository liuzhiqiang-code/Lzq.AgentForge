using FluentValidation.Validators;

namespace Lzq.AI.Application.Contracts.Commands;

public record ChatsCompletionCommand
{
    /// <summary>
    /// ChatClient
    /// </summary>
    public string ChatClient { get; set; }

    /// <summary>
    /// AIAgentModel
    /// </summary>
    public AIAgentModel AIAgentModel { get; set; }

    /// <summary>
    /// Prompt
    /// </summary>
    public string Prompt { get; set; }

}
public class AIChatsCompletionCommandValidator : MasaAbstractValidator<ChatsCompletionCommand>
{
    public AIChatsCompletionCommandValidator()
    {
    }
}