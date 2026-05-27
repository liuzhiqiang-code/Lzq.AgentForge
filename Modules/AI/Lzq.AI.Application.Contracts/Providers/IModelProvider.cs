using Lzq.AI.Domain.Enums;

namespace Lzq.AI.Application.Contracts.Providers;

public interface IModelProvider
{
    ProviderEnum Provider { get; }
    string BaseUrl { get; }
    string ModelListUrl { get; }
    List<string> ParseModels(string json);
}
