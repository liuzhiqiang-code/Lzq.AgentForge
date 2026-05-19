using Lzq.QA.Domain.Enums;
using Lzq.Extensions.SqlSugar.Repository;
using Microsoft.Extensions.DependencyInjection;
using Lzq.QA.Domain.Entities;

namespace Lzq.QA.Domain.IRepositories;

/// <summary>
/// 涓嶈壇鍝佽褰曚粨鍌ㄦ帴鍙?/// </summary>
public interface IDefectRecordRepository : ISqlSugarRepository<DefectRecordEntity>, ITransientDependency
{
    /// <summary>
    /// 鏍规嵁璐ㄦ鍗旾D鏌ヨ涓嶈壇鍝佽褰?    /// </summary>
    Task<List<DefectRecordEntity>> GetByQCOrderIdAsync(long qcOrderId);

    /// <summary>
    /// 鏍规嵁宸ュ崟ID鏌ヨ涓嶈壇鍝佽褰?    /// </summary>
    Task<List<DefectRecordEntity>> GetByWorkOrderIdAsync(long workOrderId);

    /// <summary>
    /// 鏍规嵁鐘舵€佹煡璇笉鑹搧璁板綍
    /// </summary>
    Task<List<DefectRecordEntity>> GetByStatusAsync(DefectStatusEnum status);

    /// <summary>
    /// 鑾峰彇涓嶈壇鍝佺粺璁?    /// </summary>
    Task<(int reworkCount, int scrapCount, int downgradeCount, int returnCount)> GetDefectStatisticsAsync(DateTime? fromDate, DateTime? toDate);
}
