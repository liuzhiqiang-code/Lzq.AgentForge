using Lzq.Core.Models;
using Lzq.MES.Application.Contracts.Commands;
using Lzq.MES.Application.Contracts.Queries;
using Microsoft.Extensions.DependencyInjection;
using Lzq.MES.Application.Contracts.Dtos;

namespace Lzq.MES.Application.Contracts.IServices;

/// <summary>
/// 产线服务接口
/// </summary>
public interface ILineService : ITransientDependency
{
    /// <summary>
    /// 根据ID获取产线
    /// </summary>
    Task<LineDto?> GetByIdAsync(long id);

    /// <summary>
    /// 产线分页查询
    /// </summary>
    Task<ApiResult<PagedResponse<LineDto>>> PageAsync(LinePageQuery query);

    /// <summary>
    /// 按车间获取产线列表（联动查询）
    /// </summary>
    Task<ApiResult<List<LineDto>>> ListByWorkshopAsync(long workshopId);

    /// <summary>
    /// 创建产线
    /// </summary>
    Task<ApiResult<long>> CreateAsync(LineCreateCommand command);

    /// <summary>
    /// 更新产线
    /// </summary>
    Task<ApiResult<bool>> UpdateAsync(LineUpdateCommand command);

    /// <summary>
    /// 删除产线
    /// </summary>
    Task<ApiResult<bool>> DeleteAsync(long id);

    /// <summary>
    /// 批量删除产线
    /// </summary>
    Task<ApiResult<int>> BatchDeleteAsync(List<long> ids);
}
