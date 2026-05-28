using Lzq.MES.Domain.Enums;
using Lzq.Extensions.SqlSugar.Repository;
using Microsoft.Extensions.DependencyInjection;
using Lzq.MES.Domain.Entities;

namespace Lzq.MES.Domain.IRepositories;

/// <summary>
/// 不良品记录仓储接口
/// </summary>
public interface IDefectRecordRepository : ISqlSugarRepository<DefectRecordEntity>, ITransientDependency
{
    /// <summary>
    /// 根据质检单ID查询不良品记录
    /// </summary>
    Task<List<DefectRecordEntity>> GetByQCOrderIdAsync(long qcOrderId);

    /// <summary>
    /// 根据工单ID查询不良品记录
    /// </summary>
    Task<List<DefectRecordEntity>> GetByWorkOrderIdAsync(long workOrderId);

    /// <summary>
    /// 根据状态查询不良品记录
    /// </summary>
    Task<List<DefectRecordEntity>> GetByStatusAsync(DefectStatusEnum status);

    /// <summary>
    /// 获取不良品统计
    /// </summary>
    Task<(int reworkCount, int scrapCount, int downgradeCount, int returnCount)> GetDefectStatisticsAsync(DateTime? fromDate, DateTime? toDate);
}
