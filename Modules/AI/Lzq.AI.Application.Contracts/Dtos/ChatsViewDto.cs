using Newtonsoft.Json;

namespace Lzq.AI.Application.Contracts.Dtos;

public class ChatsViewDto
{
    /// <summary>
    /// Id
    /// </summary>
    public long? Id { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string? Title { get; set; }

    public string? ChatClient { get; set; }

    /// <summary>
    /// AIAgentName
    /// </summary>
    [JsonProperty("aiAgentName")]
    public string? AIAgentName { get; set; }

    /// <summary>
    /// LastMessage
    /// </summary>
    public string? LastMessage { get; set; }

    /// <summary>
    /// 是否置顶
    /// </summary>
    public bool IsTop { get; set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime ModificationTime { get; set; }

}