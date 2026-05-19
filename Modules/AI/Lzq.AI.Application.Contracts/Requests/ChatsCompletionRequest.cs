namespace Lzq.AI.Application.Contracts.Requests;

public class ChatsCompletionRequest
{
    public long? AIChatsId { get; set; }

    public long AIModelConfigId { get; set; }

    public string AIAgentName { get; set; }

    public string Prompt { get; set; }
}
