using Lzq.Core.Models;
using Lzq.WorkOrder.Application.Contracts.WorkOrder.Commands;
using Lzq.WorkOrder.Application.Contracts.WorkOrder.Dto;
using Lzq.WorkOrder.Application.Contracts.WorkOrder.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.WorkOrder.Application.Contracts.IServices.WorkOrder;

/// <summary>
/// 工单服务接口
/// </summary>
public interface IWorkOrderService : ITransientDependency
{
    /// <summary>
    /// 工单分页查询
    /// </summary>
    Task<ApiResult<PagedResponse<WorkOrderViewDto>>> PageAsync(WorkOrderPageQuery query);

    /// <summary>
    /// 获取工单详情
    /// </summary>
    Task<ApiResult<WorkOrderViewDto>> GetAsync(long id);

    /// <summary>
    /// 创建工单
    /// </summary>
    Task<ApiResult<long>> CreateAsync(WorkOrderCreateCommand command);

    /// <summary>
    /// 更新工单
    /// </summary>
    Task<ApiResult<bool>> UpdateAsync(WorkOrderUpdateCommand command);

    /// <summary>
    /// 删除工单
    /// </summary>
    Task<ApiResult<bool>> DeleteAsync(long id);

    /// <summary>
    /// 批量删除工单
    /// </summary>
    Task<ApiResult<int>> BatchDeleteAsync(List<long> ids);

    /// <summary>
    /// 派发工单（草稿 → 已派发）
    /// </summary>
    Task<ApiResult<bool>> DispatchAsync(WorkOrderDispatchCommand command);

    /// <summary>
    /// 开工（已派发 → 生产中）
    /// </summary>
    Task<ApiResult<bool>> StartAsync(WorkOrderStartCommand command);

    /// <summary>
    /// 完工（生产中 → 已完成）
    /// </summary>
    Task<ApiResult<bool>> CompleteAsync(WorkOrderCompleteCommand command);

    /// <summary>
    /// 关闭工单（已完成 → 已关闭）
    /// </summary>
    Task<ApiResult<bool>> CloseAsync(WorkOrderCloseCommand command);

    /// <summary>
    /// 暂停工单（生产中 → 已暂停）
    /// </summary>
    Task<ApiResult<bool>> PauseAsync(WorkOrderPauseCommand command);

    /// <summary>
    /// 取消工单（已派发 → 已取消）
    /// </summary>
    Task<ApiResult<bool>> CancelAsync(WorkOrderCancelCommand command);
}
