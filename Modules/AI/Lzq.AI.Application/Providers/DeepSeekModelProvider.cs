using Lzq.AI.Application.Contracts.Providers;
using Lzq.AI.Application.Contracts.Responses;
using Lzq.AI.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Lzq.AI.Application.Providers;

public class DeepSeekModelProvider : IModelProvider, ITransientDependency
{
    public ProviderEnum Provider => ProviderEnum.DeepSeek;
    public string BaseUrl => "https://api.deepseek.com/v1";
    public string ModelListUrl => "https://api.deepseek.com/models";

    public List<string> ParseModels(string json)
    {
        var response = JsonSerializer.Deserialize<DeepSeekModelListResponse>(json);
        return response?.Data.Select(m => m.Id).ToList() ?? new List<string>();
    }
}
