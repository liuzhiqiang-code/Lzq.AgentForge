using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.IRepositories;
using Lzq.Extensions.SqlSugar.Repository;
using SqlSugar;

namespace Lzq.MES.Domain.Repositories;

/// <summary>
/// 设备台账仓储实现
/// </summary>
public class EquipmentRepository : SqlSugarRepository<EquipmentEntity>, IEquipmentRepository
{
    public EquipmentRepository() { }
    public EquipmentRepository(ISqlSugarClient client) : base(client) { }

    public async Task<EquipmentEntity?> GetByCodeAsync(string code)
    {
        return await AsQueryable()
            .Where(x => x.Code == code)
            .FirstAsync();
    }

    public async Task<List<EquipmentEntity>> GetByStatusAsync(Enums.EquipmentStatusEnum status)
    {
        return await AsQueryable()
            .Where(x => x.Status == status)
            .ToListAsync();
    }

    public async Task<List<EquipmentEntity>> GetByTypeAsync(Enums.EquipmentTypeEnum equipmentType)
    {
        return await AsQueryable()
            .Where(x => x.EquipmentType == equipmentType)
            .ToListAsync();
    }

    public async Task<List<EquipmentEntity>> GetByLineIdAsync(long lineId)
    {
        return await AsQueryable()
            .Where(x => x.LineId == lineId)
            .ToListAsync();
    }

    public async Task<bool> ExistsByCodeAsync(string code, long? excludeId = null)
    {
        var query = AsQueryable()
            .Where(x => x.Code == code);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<(int normalCount, int repairCount, int stoppedCount)> GetStatisticsAsync()
    {
        var normalCount = await AsQueryable()
            .Where(x => x.Status == Domain.Enums.EquipmentStatusEnum.Normal)
            .CountAsync();

        var repairCount = await AsQueryable()
            .Where(x => x.Status == Domain.Enums.EquipmentStatusEnum.UnderRepair)
            .CountAsync();

        var stoppedCount = await AsQueryable()
            .Where(x => x.Status == Domain.Enums.EquipmentStatusEnum.Stopped)
            .CountAsync();

        return (normalCount, repairCount, stoppedCount);
    }
}
