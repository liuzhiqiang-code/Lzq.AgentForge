using Lzq.Core.Models;
using Lzq.MES.Application.Contracts.Commands;
using Lzq.MES.Application.Contracts.Dtos;
using Lzq.MES.Application.Contracts.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.MES.Application.Contracts.IServices;

/// <summary>
/// 设备台账服务接口
/// </summary>
public interface IEquipmentService : ITransientDependency
{
    /// <summary>
    /// 设备分页查询
    /// </summary>
    Task<ApiResult<PagedResponse<EquipmentViewDto>>> PageAsync(EquipmentPageQuery query);

    /// <summary>
    /// 获取设备详情
    /// </summary>
    Task<ApiResult<EquipmentViewDto>> GetAsync(long id);

    /// <summary>
    /// 创建设备
    /// </summary>
    Task<ApiResult<long>> CreateAsync(EquipmentCreateCommand command);

    /// <summary>
    /// 更新设备
    /// </summary>
    Task<ApiResult<bool>> UpdateAsync(EquipmentUpdateCommand command);

    /// <summary>
    /// 删除设备
    /// </summary>
    Task<ApiResult<bool>> DeleteAsync(long id);

    /// <summary>
    /// 更新设备状态
    /// </summary>
    Task<ApiResult<bool>> UpdateStatusAsync(EquipmentUpdateStatusCommand command);

    /// <summary>
    /// 获取设备统计
    /// </summary>
    Task<ApiResult<EquipmentStatisticsDto>> GetStatisticsAsync();
}
