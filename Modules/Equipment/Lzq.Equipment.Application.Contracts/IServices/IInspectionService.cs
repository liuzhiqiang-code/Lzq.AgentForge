using Lzq.Core.Models;
using Lzq.Equipment.Application.Contracts.Commands;
using Lzq.Equipment.Application.Contracts.Dtos;
using Lzq.Equipment.Application.Contracts.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.Equipment.Application.Contracts.IServices;

/// <summary>
/// 点检服务接口
/// </summary>
public interface IInspectionService : ITransientDependency
{
    #region 点检计划

    /// <summary>
    /// 点检计划分页查询
    /// </summary>
    Task<ApiResult<PagedResponse<InspectionPlanViewDto>>> PlanPageAsync(InspectionPlanPageQuery query);

    /// <summary>
    /// 获取点检计划详情
    /// </summary>
    Task<ApiResult<InspectionPlanViewDto>> GetPlanAsync(long id);

    /// <summary>
    /// 获取点检计划明细
    /// </summary>
    Task<ApiResult<List<InspectionItemViewDto>>> GetPlanItemsAsync(long planId);

    /// <summary>
    /// 创建设检计划
    /// </summary>
    Task<ApiResult<long>> CreatePlanAsync(InspectionPlanCreateCommand command);

    /// <summary>
    /// 删除点检计划
    /// </summary>
    Task<ApiResult<bool>> DeletePlanAsync(long id);

    /// <summary>
    /// 获取待执行点检计划
    /// </summary>
    Task<ApiResult<List<InspectionPlanViewDto>>> GetPendingPlansAsync();

    #endregion

    #region 点检记录

    /// <summary>
    /// 点检记录分页查询
    /// </summary>
    Task<ApiResult<PagedResponse<InspectionRecordViewDto>>> RecordPageAsync(InspectionRecordPageQuery query);

    /// <summary>
    /// 获取今日点检记录
    /// </summary>
    Task<ApiResult<List<InspectionRecordViewDto>>> GetTodayRecordsAsync();

    /// <summary>
    /// 执行点检
    /// </summary>
    Task<ApiResult<bool>> ExecuteAsync(InspectionExecuteCommand command);

    #endregion
}
