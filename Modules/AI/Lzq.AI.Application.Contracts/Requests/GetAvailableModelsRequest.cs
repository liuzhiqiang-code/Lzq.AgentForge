using Lzq.AI.Domain.Enums;

namespace Lzq.AI.Application.Contracts.Requests;

public record GetAvailableModelsRequest(ProviderEnum Provider, string KeyValue);