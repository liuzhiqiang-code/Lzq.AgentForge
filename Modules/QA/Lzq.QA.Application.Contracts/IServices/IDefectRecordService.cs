using Lzq.Core.Models;
using Lzq.QA.Application.Contracts.Commands;
using Lzq.QA.Application.Contracts.Dtos;
using Lzq.QA.Application.Contracts.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.QA.Application.Contracts.IServices;

/// <summary>
/// 涓嶈壇鍝佹湇鍔℃帴鍙?/// </summary>
public interface IDefectRecordService : ITransientDependency
{
    #region 鏌ヨ

    /// <summary>
    /// 涓嶈壇鍝佸垎椤垫煡璇?    /// </summary>
    Task<ApiResult<PagedResponse<DefectRecordViewDto>>> PageAsync(DefectRecordPageQuery query);

    /// <summary>
    /// 鑾峰彇涓嶈壇鍝佽鎯?    /// </summary>
    Task<ApiResult<DefectRecordViewDto>> GetAsync(long id);

    /// <summary>
    /// 鑾峰彇璐ㄦ鍗曞叧鑱旂殑涓嶈壇鍝佽褰?    /// </summary>
    Task<ApiResult<List<DefectRecordViewDto>>> GetByQCOrderIdAsync(long qcOrderId);

    /// <summary>
    /// 鑾峰彇涓嶈壇鍝佺粺璁?    /// </summary>
    Task<ApiResult<DefectStatisticsDto>> GetStatisticsAsync(DateTime? fromDate, DateTime? toDate);

    #endregion

    #region CRUD

    /// <summary>
    /// 鍒涘缓涓嶈壇鍝佽褰?    /// </summary>
    Task<ApiResult<long>> CreateAsync(DefectRecordCreateCommand command);

    /// <summary>
    /// 鍒犻櫎涓嶈壇鍝佽褰?    /// </summary>
    Task<ApiResult<bool>> DeleteAsync(long id);

    #endregion

    #region 澶勭悊娴佺▼

    /// <summary>
    /// 澶勭悊涓嶈壇鍝?    /// </summary>
    Task<ApiResult<bool>> HandleAsync(DefectRecordHandleCommand command);

    /// <summary>
    /// 璇勫涓嶈壇鍝?    /// </summary>
    Task<ApiResult<bool>> ReviewAsync(DefectRecordReviewCommand command);

    #endregion
}
