using System.Text.Json.Serialization;

namespace Lzq.AI.Application.Contracts.Responses;

public class SiliconFlowModelListResponse
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public List<SiliconFlowModel> Data { get; set; } = new();
}

public class SiliconFlowModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("owned_by")]
    public string OwnedBy { get; set; } = string.Empty;
}