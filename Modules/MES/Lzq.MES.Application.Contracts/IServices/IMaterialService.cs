using Lzq.MES.Application.Contracts.Commands;
using Lzq.MES.Application.Contracts.Dtos;
using Lzq.MES.Application.Contracts.Queries;
using Lzq.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.MES.Application.Contracts.IServices;

public interface IMaterialService : ITransientDependency
{
    Task<ApiResult<PagedResponse<MaterialDto>>> PageAsync(MaterialPageQuery query);
    Task<ApiResult<MaterialDto>> GetAsync(long id);
    Task<ApiResult<List<MaterialDto>>> SelectListAsync();
    Task<ApiResult<long>> CreateAsync(MaterialCreateCommand command);
    Task<ApiResult<bool>> UpdateAsync(MaterialUpdateCommand command);
    Task<ApiResult<bool>> DeleteAsync(long id);
    Task<ApiResult<int>> BatchDeleteAsync(List<long> ids);
}

