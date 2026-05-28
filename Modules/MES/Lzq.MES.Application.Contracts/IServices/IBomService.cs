using Lzq.MES.Application.Contracts.Commands;
using Lzq.MES.Application.Contracts.Dtos;
using Lzq.MES.Application.Contracts.Queries;
using Lzq.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.MES.Application.Contracts.IServices;

public interface IBomService : ITransientDependency
{
    Task<ApiResult<PagedResponse<BomDto>>> PageAsync(BomPageQuery query);
    Task<ApiResult<BomDetailDto>> GetAsync(long id);
    Task<ApiResult<long>> CreateAsync(BomCreateCommand command);
    Task<ApiResult<bool>> UpdateAsync(BomUpdateCommand command);
    Task<ApiResult<bool>> DeleteAsync(long id);
    Task<ApiResult<long>> CopyAsync(long id);
    Task<ApiResult<bool>> ReleaseAsync(long id, string? changeDescription = null);
    Task<ApiResult<bool>> RollbackAsync(long bomId, long historyId);

    Task<ApiResult<List<BomItemDto>>> GetItemsAsync(long bomId);
    Task<ApiResult<long>> CreateItemAsync(BomItemCreateCommand command);
    Task<ApiResult<bool>> UpdateItemAsync(BomItemUpdateCommand command);
    Task<ApiResult<bool>> DeleteItemAsync(long id);

    Task<ApiResult<List<BomVersionHistoryDto>>> GetVersionHistoryAsync(long bomId);
    Task<ApiResult<BomDiffResultDto>> DiffVersionsAsync(long historyId1, long historyId2);
    Task<ApiResult<BomDetailDto>> PreviewVersionAsync(long historyId);
}

