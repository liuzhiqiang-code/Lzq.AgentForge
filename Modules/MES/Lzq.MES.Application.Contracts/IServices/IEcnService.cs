using Lzq.MES.Application.Contracts.Commands;
using Lzq.MES.Application.Contracts.Dtos;
using Lzq.MES.Application.Contracts.Queries;
using Lzq.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.MES.Application.Contracts.IServices;

public interface IEcnService : ITransientDependency
{
    Task<ApiResult<PagedResponse<EcnDto>>> PageAsync(EcnPageQuery query);
    Task<ApiResult<EcnDetailDto>> GetAsync(long id);
    Task<ApiResult<long>> CreateAsync(EcnCreateCommand command);
    Task<ApiResult<bool>> UpdateAsync(EcnUpdateCommand command);
    Task<ApiResult<bool>> DeleteAsync(long id);

    Task<ApiResult<List<EcnItemDto>>> GetItemsAsync(long ecnId);
    Task<ApiResult<long>> CreateItemAsync(EcnItemCreateCommand command);
    Task<ApiResult<bool>> DeleteItemAsync(long id);

    Task<ApiResult<bool>> SubmitAsync(long id);
    Task<ApiResult<bool>> ApproveAsync(long id);
    Task<ApiResult<bool>> ExecuteAsync(EcnExecuteCommand command);
    Task<ApiResult<bool>> ConfirmAsync(long id);
    Task<ApiResult<bool>> CancelAsync(long id);

    Task<ApiResult<string>> AnalyzeImpactAsync(long id);
}
