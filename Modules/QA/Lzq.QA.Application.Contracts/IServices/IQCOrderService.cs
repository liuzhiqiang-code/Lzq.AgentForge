using Lzq.Core.Models;
using Lzq.QA.Application.Contracts.Commands;
using Lzq.QA.Application.Contracts.Dtos;
using Lzq.QA.Application.Contracts.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.QA.Application.Contracts.IServices;

/// <summary>
/// 质检订单服务接口（支持IQC/PQC/OQC）
/// </summary>
public interface IQCOrderService : ITransientDependency
{
    #region 鏌ヨ

    /// <summary>
    /// 璐ㄦ鍗曞垎椤垫煡璇?    /// </summary>
    Task<ApiResult<PagedResponse<QCOrderViewDto>>> PageAsync(QCOrderPageQuery query);

    /// <summary>
    /// 鑾峰彇璐ㄦ鍗曡鎯?    /// </summary>
    Task<ApiResult<QCOrderViewDto>> GetAsync(long id);

    /// <summary>
    /// 鑾峰彇璐ㄦ鍗曠殑妫€楠屾槑缁?    /// </summary>
    Task<ApiResult<List<QCOrderItemViewDto>>> GetItemsAsync(long qcOrderId);

    #endregion

    #region CRUD

    /// <summary>
    /// 鍒涘缓璐ㄦ鍗?    /// </summary>
    Task<ApiResult<long>> CreateAsync(QCOrderCreateCommand command);

    /// <summary>
    /// 鏇存柊璐ㄦ鍗?    /// </summary>
    Task<ApiResult<bool>> UpdateAsync(QCOrderUpdateCommand command);

    /// <summary>
    /// 鍒犻櫎璐ㄦ鍗?    /// </summary>
    Task<ApiResult<bool>> DeleteAsync(long id);

    #endregion

    #region 妫€楠屾祦绋?
    /// <summary>
    /// 鎻愪氦妫€楠岀粨鏋?    /// </summary>
    Task<ApiResult<bool>> SubmitInspectAsync(QCOrderSubmitInspectCommand command);

    /// <summary>
    /// 鍒ゅ畾璐ㄦ鍗?    /// </summary>
    Task<ApiResult<bool>> JudgeAsync(QCOrderJudgeCommand command);

    /// <summary>
    /// 鍙栨秷璐ㄦ鍗?    /// </summary>
    Task<ApiResult<bool>> CancelAsync(QCOrderCancelCommand command);

    #endregion
}
