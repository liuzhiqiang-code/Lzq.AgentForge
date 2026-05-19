using Lzq.AI.Domain.Enums;
using Lzq.Core.Models;

namespace Lzq.AI.Application.Contracts.Queries;

public record ChatHistoryPageQuery : PagedRequest
{
    /// <summary>
    /// Id
    /// </summary>
    public long? Id { get; set; }

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

    public ChatHistoryPageQuery()
    {
    }
}