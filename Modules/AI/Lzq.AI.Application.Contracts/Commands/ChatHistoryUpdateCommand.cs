using FluentValidation;
using FluentValidation.Validators;
using Lzq.AI.Domain.Enums;

namespace Lzq.AI.Application.Contracts.Commands;

public record ChatHistoryUpdateCommand
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

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
public class AIChatHistoryUpdateCommandValidator : MasaAbstractValidator<ChatHistoryUpdateCommand>
{
    public AIChatHistoryUpdateCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("ID必须大于0");
    }
}