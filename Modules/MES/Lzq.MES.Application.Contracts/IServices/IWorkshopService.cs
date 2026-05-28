using Lzq.Core.Models;
using Lzq.MES.Application.Contracts.Commands;
using Lzq.MES.Application.Contracts.Queries;
using Microsoft.Extensions.DependencyInjection;
using Lzq.MES.Application.Contracts.Dtos;

namespace Lzq.MES.Application.Contracts.IServices;

/// <summary>
/// 车间服务接口
/// </summary>
public interface IWorkshopService : ITransientDependency
{
    /// <summary>
    /// 车间分页查询
    /// </summary>
    Task<ApiResult<PagedResponse<WorkshopDto>>> PageAsync(WorkshopPageQuery query);

    /// <summary>
    /// 按工厂获取车间列表（联动查询）
    /// </summary>
    Task<ApiResult<List<WorkshopDto>>> ListByFactoryAsync(long factoryId);

    /// <summary>
    /// 获取车间树（车间→产线→工序）
    /// </summary>
    Task<ApiResult<List<WorkshopTreeDto>>> TreeAsync(long factoryId);

    /// <summary>
    /// 创建车间
    /// </summary>
    Task<ApiResult<long>> CreateAsync(WorkshopCreateCommand command);

    /// <summary>
    /// 更新车间
    /// </summary>
    Task<ApiResult<bool>> UpdateAsync(WorkshopUpdateCommand command);

    /// <summary>
    /// 删除车间
    /// </summary>
    Task<ApiResult<bool>> DeleteAsync(long id);

    /// <summary>
    /// 批量删除车间
    /// </summary>
    Task<ApiResult<int>> BatchDeleteAsync(List<long> ids);
}
