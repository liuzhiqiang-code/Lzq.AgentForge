using Microsoft.AspNetCore.Builder;
using Lzq.MES.Domain.Enums;
using Lzq.MES.Domain.IRepositories;
using Lzq.MES.Application.Contracts.IServices;
using Lzq.MES.Application.Contracts.Dtos;

namespace Lzq.MES.Application.Services;

/// <summary>
/// 质检统计服务实现（供Dashboard模块使用）
/// </summary>
public class QCStatisticsService : ServiceBase, IQCStatisticsService
{
    public QCStatisticsService() : base("/api/v1/mes/qc/statistics")
    {
    }

    private IQCOrderRepository QCOrderRepo => GetRequiredService<IQCOrderRepository>();

    /// <inheritdoc/>
    public async Task<DefectRateStatisticsResultDto> GetDefectRateStatisticsAsync(DateTime start, DateTime end, long? lineId = null)
    {
        // 查询指定时间范围内的PQC（过程检验）质检单
        var query = QCOrderRepo.AsQueryable()
            .Where(q => q.InspectDate >= start && q.InspectDate < end)
            .Where(q => q.QCType == QCTypeEnum.PQC); // 只统计过程检验
        var submitQty = await query.SumAsync(q => q.SubmitQty);
        var qualifiedQty = await query.SumAsync(q => q.QualifiedQty);
        var unqualifiedQty = await query.SumAsync(q => q.UnqualifiedQty);

        if (submitQty == 0)
        {
            return new DefectRateStatisticsResultDto
            {
                SubmitQty = 0,
                QualifiedQty = 0,
                UnqualifiedQty = 0,
                QualifiedRate = 100m, // 无检验数据时默认为100%
                DefectRate = 0m
            };
        }

        var qualifiedRate = Math.Round((decimal)qualifiedQty / submitQty * 100, 2);
        var defectRate = Math.Round((decimal)unqualifiedQty / submitQty * 100, 2);

        return new DefectRateStatisticsResultDto
        {
            SubmitQty = submitQty,
            QualifiedQty = qualifiedQty,
            UnqualifiedQty = unqualifiedQty,
            QualifiedRate = qualifiedRate,
            DefectRate = defectRate
        };
    }
}
