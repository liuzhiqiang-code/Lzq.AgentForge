using Lzq.QA.Domain.Enums;
using Lzq.Extensions.SqlSugar.Repository;
using Microsoft.Extensions.DependencyInjection;
using Lzq.QA.Domain.Entities;

namespace Lzq.QA.Domain.IRepositories;

/// <summary>
/// 璐ㄦ鍗曚粨鍌ㄦ帴鍙?/// </summary>
public interface IQCOrderRepository : ISqlSugarRepository<QCOrderEntity>, ITransientDependency
{
    /// <summary>
    /// 鏍规嵁缂栧彿鏌ヨ
    /// </summary>
    Task<QCOrderEntity?> GetByCodeAsync(string code);

    /// <summary>
    /// 鏍规嵁绫诲瀷鍜岀姸鎬佹煡璇㈣川妫€鍗?    /// </summary>
    Task<List<QCOrderEntity>> GetByTypeAndStatusAsync(QCTypeEnum qcType, QCOrderStatusEnum status);

    /// <summary>
    /// 鏍规嵁鍏宠仈鍗曟嵁鏌ヨ璐ㄦ鍗?    /// </summary>
    Task<List<QCOrderEntity>> GetByRefIdAsync(long refId);

    /// <summary>
    /// 妫€鏌ヨ川妫€鍗曠紪鍙锋槸鍚﹀瓨鍦?    /// </summary>
    Task<bool> ExistsByCodeAsync(string code, long? excludeId = null);
}

/// <summary>
/// 璐ㄦ鏄庣粏浠撳偍鎺ュ彛
/// </summary>
public interface IQCOrderItemRepository : ISqlSugarRepository<QCOrderItemEntity>, ITransientDependency
{
    /// <summary>
    /// 鏍规嵁璐ㄦ鍗旾D鏌ヨ鎵€鏈夋槑缁?    /// </summary>
    Task<List<QCOrderItemEntity>> GetByQCOrderIdAsync(long qcOrderId);

    /// <summary>
    /// 鍒犻櫎鎸囧畾璐ㄦ鍗曠殑鎵€鏈夋槑缁?    /// </summary>
    Task DeleteByQCOrderIdAsync(long qcOrderId);
}
