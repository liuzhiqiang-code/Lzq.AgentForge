namespace Lzq.AI.Application.Contracts.Queries;

public record ChatsListQuery
{
    /// <summary>
    /// Id
    /// </summary>
    public long? Id { get; set; }

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

    public ChatsListQuery()
    {
    }
}