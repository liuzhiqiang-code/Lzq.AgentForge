using Lzq.AI.Application.Contracts.Commands;
using Lzq.AI.Application.Contracts.Queries;
using Lzq.AI.Application.Contracts.Requests;
using Lzq.Extensions.AI;

namespace Lzq.AI.Application.Contracts.IServices;

public interface IApiKeyService
{
    Task<ApiResult> PageAsync(ApiKeyPageQuery query);
    Task<ApiResult> ListAsync();
    Task<ApiResult> DetailAsync(long id);
    Task<ApiResult<List<string>>> GetAvailableModelsAsync(GetAvailableModelsRequest request);
    Task<ApiResult> CreateAsync(ApiKeyCreateCommand command);
    Task<ApiResult> UpdateAsync(ApiKeyUpdateCommand command);
    Task<ApiResult> DeleteAsync(long id);
    Task<ApiResult> BatchDeleteAsync(List<long> ids);
    Task<AISetting> GetAISettingAsync(long aIModelConfigId);
}