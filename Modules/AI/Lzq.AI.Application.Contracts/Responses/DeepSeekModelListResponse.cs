using System.Text.Json.Serialization;

namespace Lzq.AI.Application.Contracts.Responses;

public class DeepSeekModelListResponse
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public List<DeepSeekModel> Data { get; set; } = new();
}

public class DeepSeekModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("owned_by")]
    public string OwnedBy { get; set; } = string.Empty;
}
