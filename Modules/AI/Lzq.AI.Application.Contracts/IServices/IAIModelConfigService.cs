using Lzq.AI.Application.Contracts.Commands;
using Lzq.AI.Application.Contracts.Queries;

namespace Lzq.AI.Application.Contracts.IServices;

public interface IAIModelConfigService
{
    Task<ApiResult> PageAsync(ModelConfigPageQuery query);
    Task<ApiResult> ListAsync();
    Task<ApiResult> CreateAsync(ModelConfigCreateCommand command);
    Task<ApiResult> UpdateAsync(ModelConfigUpdateCommand command);
    Task<ApiResult> DeleteAsync(long id);
    Task<ApiResult> BatchDeleteAsync(List<long> ids);
}