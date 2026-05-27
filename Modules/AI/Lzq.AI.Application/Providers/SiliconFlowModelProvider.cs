using Lzq.AI.Application.Contracts.Providers;
using Lzq.AI.Application.Contracts.Responses;
using Lzq.AI.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Lzq.AI.Application.Providers;

public class SiliconFlowModelProvider : IModelProvider, ITransientDependency
{
    public ProviderEnum Provider => ProviderEnum.SiliconFlow;
    public string BaseUrl => "https://api.siliconflow.cn/v1";
    public string ModelListUrl => "https://api.siliconflow.cn/v1/models";

    public List<string> ParseModels(string json)
    {
        var response = JsonSerializer.Deserialize<SiliconFlowModelListResponse>(json);
        return response?.Data.Select(m => m.Id).ToList() ?? new List<string>();
    }
}
