using Lzq.Extensions.SqlSugar.Repository;
using Lzq.QA.Domain.Entities;
using Lzq.QA.Domain.IRepositories;
using SqlSugar;

namespace Lzq.QA.Domain.Repositories;

/// <summary>
/// 涓嶈壇鍝佽褰曚粨鍌ㄥ疄鐜?/// </summary>
public class DefectRecordRepository : SqlSugarRepository<DefectRecordEntity>, IDefectRecordRepository
{
    public DefectRecordRepository()
    {
    }
    public DefectRecordRepository(ISqlSugarClient client) : base(client) { }

    public async Task<List<DefectRecordEntity>> GetByQCOrderIdAsync(long qcOrderId)
    {
        return await AsQueryable()
            .Where(x => x.QCOrderId == qcOrderId)
            .ToListAsync();
    }

    public async Task<List<DefectRecordEntity>> GetByWorkOrderIdAsync(long workOrderId)
    {
        return await AsQueryable()
            .Where(x => x.WorkOrderId == workOrderId)
            .ToListAsync();
    }

    public async Task<List<DefectRecordEntity>> GetByStatusAsync(Enums.DefectStatusEnum status)
    {
        return await AsQueryable()
            .Where(x => x.Status == status)
            .ToListAsync();
    }

    public async Task<(int reworkCount, int scrapCount, int downgradeCount, int returnCount)> GetDefectStatisticsAsync(DateTime? fromDate, DateTime? toDate)
    {
        var query = AsQueryable();

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.CreationTime >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.CreationTime <= toDate.Value);
        }

        var records = await query.ToListAsync();

        var reworkCount = records.Count(x => x.HandlingType == Domain.Enums.DefectHandlingEnum.Rework);
        var scrapCount = records.Count(x => x.HandlingType == Domain.Enums.DefectHandlingEnum.Scrap);
        var downgradeCount = records.Count(x => x.HandlingType == Domain.Enums.DefectHandlingEnum.Downgrade);
        var returnCount = records.Count(x => x.HandlingType == Domain.Enums.DefectHandlingEnum.Return);

        return (reworkCount, scrapCount, downgradeCount, returnCount);
    }
}
