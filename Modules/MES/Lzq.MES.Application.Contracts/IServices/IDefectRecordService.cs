using Lzq.Core.Models;
using Lzq.MES.Application.Contracts.Commands;
using Lzq.MES.Application.Contracts.Dtos;
using Lzq.MES.Application.Contracts.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.MES.Application.Contracts.IServices;

/// <summary>
/// 不良品服务接口
/// </summary>
public interface IDefectRecordService : ITransientDependency
{
    #region 查询

    /// <summary>
    /// 不良品分页查询
    /// </summary>
    Task<ApiResult<PagedResponse<DefectRecordViewDto>>> PageAsync(DefectRecordPageQuery query);

    /// <summary>
    /// 获取不良品详情
    /// </summary>
    Task<ApiResult<DefectRecordViewDto>> GetAsync(long id);

    /// <summary>
    /// 获取质检单关联的不良品记录
    /// </summary>
    Task<ApiResult<List<DefectRecordViewDto>>> GetByQCOrderIdAsync(long qcOrderId);

    /// <summary>
    /// 获取不良品统计
    /// </summary>
    Task<ApiResult<DefectStatisticsDto>> GetStatisticsAsync(DateTime? fromDate, DateTime? toDate);

    #endregion

    #region CRUD

    /// <summary>
    /// 创建不良品记录
    /// </summary>
    Task<ApiResult<long>> CreateAsync(DefectRecordCreateCommand command);

    /// <summary>
    /// 删除不良品记录
    /// </summary>
    Task<ApiResult<bool>> DeleteAsync(long id);

    #endregion

    #region 处理流程

    /// <summary>
    /// 处理不良品
    /// </summary>
    Task<ApiResult<bool>> HandleAsync(DefectRecordHandleCommand command);

    /// <summary>
    /// 评审不良品
    /// </summary>
    Task<ApiResult<bool>> ReviewAsync(DefectRecordReviewCommand command);

    #endregion
}
