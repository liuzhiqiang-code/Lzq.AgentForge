using Lzq.Core.Models;
using Lzq.MES.Application.Contracts.Commands;
using Lzq.MES.Application.Contracts.Dtos;
using Lzq.MES.Application.Contracts.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.MES.Application.Contracts.IServices;

/// <summary>
/// 维修服务接口
/// </summary>
public interface IRepairOrderService : ITransientDependency
{
    /// <summary>
    /// 报修单分页查询
    /// </summary>
    Task<ApiResult<PagedResponse<RepairOrderViewDto>>> PageAsync(RepairOrderPageQuery query);

    /// <summary>
    /// 获取报修单详情
    /// </summary>
    Task<ApiResult<RepairOrderViewDto>> GetAsync(long id);

    /// <summary>
    /// 创建报修单
    /// </summary>
    Task<ApiResult<long>> CreateAsync(RepairOrderCreateCommand command);

    /// <summary>
    /// 派工
    /// </summary>
    Task<ApiResult<bool>> AssignAsync(RepairAssignCommand command);

    /// <summary>
    /// 维修开始
    /// </summary>
    Task<ApiResult<bool>> StartAsync(RepairStartCommand command);

    /// <summary>
    /// 维修完成
    /// </summary>
    Task<ApiResult<bool>> CompleteAsync(RepairCompleteCommand command);

    /// <summary>
    /// 验收
    /// </summary>
    Task<ApiResult<bool>> AcceptAsync(RepairAcceptCommand command);

    /// <summary>
    /// 取消报修单
    /// </summary>
    Task<ApiResult<bool>> CancelAsync(long id);

    /// <summary>
    /// 获取维修统计
    /// </summary>
    Task<ApiResult<RepairStatisticsDto>> GetStatisticsAsync(DateTime? fromDate, DateTime? toDate);
}

/// <summary>
/// 保养服务接口
/// </summary>
public interface IMaintenanceService : ITransientDependency
{
    #region 保养计划

    /// <summary>
    /// 保养计划分页查询
    /// </summary>
    Task<ApiResult<PagedResponse<MaintenancePlanViewDto>>> PlanPageAsync(MaintenancePlanPageQuery query);

    /// <summary>
    /// 获取保养计划详情
    /// </summary>
    Task<ApiResult<MaintenancePlanViewDto>> GetPlanAsync(long id);

    /// <summary>
    /// 创保养计划
    /// </summary>
    Task<ApiResult<long>> CreatePlanAsync(MaintenancePlanCreateCommand command);

    /// <summary>
    /// 删除保养计划
    /// </summary>
    Task<ApiResult<bool>> DeletePlanAsync(long id);

    #endregion

    #region 保养记录

    /// <summary>
    /// 保养记录分页查询
    /// </summary>
    Task<ApiResult<PagedResponse<MaintenanceRecordViewDto>>> RecordPageAsync(MaintenanceRecordPageQuery query);

    /// <summary>
    /// 创建保养记录
    /// </summary>
    Task<ApiResult<long>> CreateRecordAsync(MaintenanceRecordCreateCommand command);

    #endregion
}
