using Lzq.MES.Application.Contracts.Commands;
using Lzq.MES.Application.Contracts.Dtos;
using Lzq.MES.Application.Contracts.Queries;
using Lzq.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.MES.Application.Contracts.IServices;

public interface IMaterialTypeService : ITransientDependency
{
    Task<ApiResult<PagedResponse<MaterialTypeDto>>> PageAsync(MaterialTypePageQuery query);
    Task<ApiResult<List<MaterialTypeTreeDto>>> TreeAsync();
    Task<ApiResult<long>> CreateAsync(MaterialTypeCreateCommand command);
    Task<ApiResult<bool>> UpdateAsync(MaterialTypeUpdateCommand command);
    Task<ApiResult<bool>> DeleteAsync(long id);
}

