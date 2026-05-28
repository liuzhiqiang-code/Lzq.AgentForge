using Lzq.Core.Models;
using Lzq.MES.Application.Contracts.Commands;
using Lzq.MES.Application.Contracts.Queries;
using Microsoft.Extensions.DependencyInjection;
using Lzq.MES.Application.Contracts.Dtos;

namespace Lzq.MES.Application.Contracts.IServices;

/// <summary>
/// 工厂服务接口
/// </summary>
public interface IFactoryService : ITransientDependency
{
    /// <summary>
    /// 工厂分页查询
    /// </summary>
    Task<ApiResult<PagedResponse<FactoryDto>>> PageAsync(FactoryPageQuery query);

    /// <summary>
    /// 获取完整工厂树（工厂→车间→产线→工序）
    /// </summary>
    Task<ApiResult<List<FactoryTreeDto>>> TreeAsync();

    /// <summary>
    /// 创建工厂
    /// </summary>
    Task<ApiResult<long>> CreateAsync(FactoryCreateCommand command);

    /// <summary>
    /// 更新工厂
    /// </summary>
    Task<ApiResult<bool>> UpdateAsync(FactoryUpdateCommand command);

    /// <summary>
    /// 删除工厂
    /// </summary>
    Task<ApiResult<bool>> DeleteAsync(long id);

    /// <summary>
    /// 批量删除工厂
    /// </summary>
    Task<ApiResult<int>> BatchDeleteAsync(List<long> ids);
}
