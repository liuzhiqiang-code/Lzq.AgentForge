using FluentValidation.Validators;

namespace Lzq.AI.Application.Contracts.Commands;

public record ChatsCreateCommand
{
    /// <summary>
    /// AIChatsName
    /// </summary>
    public string? AIChatsName { get; set; }

    /// <summary>
    /// LastMessage
    /// </summary>
    public string? LastMessage { get; set; }

    /// <summary>
    /// UserId
    /// </summary>
    public long? UserId { get; set; }

}
public class AIChatsCreateCommandValidator : MasaAbstractValidator<ChatsCreateCommand>
{
    public AIChatsCreateCommandValidator()
    {
    }
}