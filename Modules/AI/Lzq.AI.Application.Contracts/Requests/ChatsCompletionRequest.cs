namespace Lzq.AI.Application.Contracts.Requests;

public class ChatsCompletionRequest
{
    public long? ChatsId { get; set; }

    public long AIModelConfigId { get; set; }

    public string AIAgentName { get; set; }

    public string Prompt { get; set; }
}
