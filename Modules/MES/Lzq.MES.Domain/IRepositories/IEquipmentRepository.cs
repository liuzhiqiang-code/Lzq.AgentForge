using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.Enums;
using Lzq.Extensions.SqlSugar.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.MES.Domain.IRepositories;

/// <summary>
/// 设备台账仓储接口
/// </summary>
public interface IEquipmentRepository : ISqlSugarRepository<EquipmentEntity>, ITransientDependency
{
    /// <summary>
    /// 根据编号查询
    /// </summary>
    Task<EquipmentEntity?> GetByCodeAsync(string code);

    /// <summary>
    /// 根据状态查询设备
    /// </summary>
    Task<List<EquipmentEntity>> GetByStatusAsync(EquipmentStatusEnum status);

    /// <summary>
    /// 根据类型查询设备
    /// </summary>
    Task<List<EquipmentEntity>> GetByTypeAsync(EquipmentTypeEnum equipmentType);

    /// <summary>
    /// 根据产线查询设备
    /// </summary>
    Task<List<EquipmentEntity>> GetByLineIdAsync(long lineId);

    /// <summary>
    /// 检查设备编号是否存在
    /// </summary>
    Task<bool> ExistsByCodeAsync(string code, long? excludeId = null);

    /// <summary>
    /// 获取设备统计
    /// </summary>
    Task<(int normalCount, int repairCount, int stoppedCount)> GetStatisticsAsync();
}
