using Lzq.Core.Models;
using Lzq.MES.Application.Contracts.Commands;
using Lzq.MES.Application.Contracts.Queries;
using Microsoft.Extensions.DependencyInjection;
using Lzq.MES.Application.Contracts.Dtos;

namespace Lzq.MES.Application.Contracts.IServices;

/// <summary>
/// 工序服务接口
/// </summary>
public interface IProcessService : ITransientDependency
{
    /// <summary>
    /// 工序分页查询
    /// </summary>
    Task<ApiResult<PagedResponse<ProcessDto>>> PageAsync(ProcessPageQuery query);

    /// <summary>
    /// 按产线获取工序列表（联动查询）
    /// </summary>
    Task<ApiResult<List<ProcessDto>>> ListByLineAsync(long lineId);

    /// <summary>
    /// 创建工序
    /// </summary>
    Task<ApiResult<long>> CreateAsync(ProcessCreateCommand command);

    /// <summary>
    /// 更新工序
    /// </summary>
    Task<ApiResult<bool>> UpdateAsync(ProcessUpdateCommand command);

    /// <summary>
    /// 删除工序
    /// </summary>
    Task<ApiResult<bool>> DeleteAsync(long id);

    /// <summary>
    /// 批量删除工序
    /// </summary>
    Task<ApiResult<int>> BatchDeleteAsync(List<long> ids);
}
