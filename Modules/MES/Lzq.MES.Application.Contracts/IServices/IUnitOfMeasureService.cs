using Lzq.MES.Application.Contracts.Commands;
using Lzq.MES.Application.Contracts.Dtos;
using Lzq.MES.Application.Contracts.Queries;
using Lzq.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.MES.Application.Contracts.IServices;

public interface IUnitOfMeasureService : ITransientDependency
{
    Task<ApiResult<PagedResponse<UnitOfMeasureDto>>> PageAsync(UnitOfMeasurePageQuery query);
    Task<ApiResult<List<UnitOfMeasureDto>>> SelectListAsync();
    Task<ApiResult<long>> CreateAsync(UnitOfMeasureCreateCommand command);
    Task<ApiResult<bool>> UpdateAsync(UnitOfMeasureUpdateCommand command);
    Task<ApiResult<bool>> DeleteAsync(long id);
    Task<ApiResult<int>> BatchDeleteAsync(List<long> ids);
}

