using FluentValidation.Validators;
using Lzq.AI.Domain.Enums;

namespace Lzq.AI.Application.Contracts.Commands;

public record ChatHistoryCreateCommand
{
    /// <summary>
    /// AIChatsId
    /// </summary>
    public long? AIChatsId { get; set; }

    /// <summary>
    /// Content
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// MessageType
    /// </summary>
    public MessageTypeEnum? MessageType { get; set; }

}
public class AIChatHistoryCreateCommandValidator : MasaAbstractValidator<ChatHistoryCreateCommand>
{
    public AIChatHistoryCreateCommandValidator()
    {
    }
}