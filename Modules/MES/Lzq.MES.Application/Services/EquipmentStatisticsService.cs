using Lzq.MES.Application.Contracts.Dtos;
using Lzq.MES.Application.Contracts.IServices;
using Lzq.MES.Domain.Enums;
using Lzq.MES.Domain.IRepositories;
using Microsoft.AspNetCore.Builder;

namespace Lzq.MES.Application.Services;

/// <summary>
/// 设备统计服务实现（供Dashboard模块使用）
/// </summary>
public class EquipmentStatisticsService : ServiceBase, IEquipmentStatisticsService
{
    public EquipmentStatisticsService() : base("/api/v1/mes/equipment/statistics")
    {
    }

    private IEquipmentRepository EquipmentRepo => GetRequiredService<IEquipmentRepository>();

    /// <inheritdoc/>
    public async Task<EquipmentStatusOverviewResultDto> GetStatusOverviewAsync()
    {
        var totalCount = await EquipmentRepo.AsQueryable().CountAsync();
        
        if (totalCount == 0)
        {
            return new EquipmentStatusOverviewResultDto
            {
                TotalCount = 0,
                NormalCount = 0,
                UnderRepairCount = 0,
                UnderMaintenanceCount = 0,
                StoppedCount = 0,
                NormalRate = 0
            };
        }

        var normalCount = await EquipmentRepo.AsQueryable()
            .Where(e => e.Status == EquipmentStatusEnum.Normal)
            .CountAsync();

        var underRepairCount = await EquipmentRepo.AsQueryable()
            .Where(e => e.Status == EquipmentStatusEnum.UnderRepair)
            .CountAsync();

        var underMaintenanceCount = await EquipmentRepo.AsQueryable()
            .Where(e => e.Status == EquipmentStatusEnum.UnderMaintenance)
            .CountAsync();

        var stoppedCount = await EquipmentRepo.AsQueryable()
            .Where(e => e.Status == EquipmentStatusEnum.Stopped)
            .CountAsync();

        return new EquipmentStatusOverviewResultDto
        {
            TotalCount = totalCount,
            NormalCount = normalCount,
            UnderRepairCount = underRepairCount,
            UnderMaintenanceCount = underMaintenanceCount,
            StoppedCount = stoppedCount,
            NormalRate = Math.Round((decimal)normalCount / totalCount * 100, 2)
        };
    }

    /// <inheritdoc/>
    public async Task<List<EquipmentStatusByLineResultDto>> GetStatusByLineAsync()
    {
        // 获取所有有产线信息的设备分组
        var lineGroups = await EquipmentRepo.AsQueryable()
            .Where(e => e.LineId != null)
            .GroupBy(e => new { e.LineId, e.LineName })
            .Select(g => new { g.LineId, g.LineName })
            .ToListAsync();

        var result = new List<EquipmentStatusByLineResultDto>();

        foreach (var line in lineGroups)
        {
            var lineTotal = await EquipmentRepo.AsQueryable()
                .Where(e => e.LineId == line.LineId)
                .CountAsync();

            var lineNormal = await EquipmentRepo.AsQueryable()
                .Where(e => e.LineId == line.LineId && e.Status == EquipmentStatusEnum.Normal)
                .CountAsync();

            result.Add(new EquipmentStatusByLineResultDto
            {
                LineId = line.LineId,
                LineName = line.LineName,
                TotalCount = lineTotal,
                NormalCount = lineNormal,
                AbnormalCount = lineTotal - lineNormal,
                NormalRate = lineTotal > 0 ? Math.Round((decimal)lineNormal / lineTotal * 100, 2) : 0
            });
        }

        return result;
    }
}
