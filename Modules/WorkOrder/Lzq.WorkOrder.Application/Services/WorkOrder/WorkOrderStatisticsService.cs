using Microsoft.AspNetCore.Builder;
using Lzq.WorkOrder.Application.Contracts.IServices.WorkOrder;
using Lzq.WorkOrder.Application.Contracts.WorkOrder.Dto;
using Lzq.WorkOrder.Domain.Entities.WorkOrder;
using Lzq.WorkOrder.Domain.Enums;
using Lzq.WorkOrder.Domain.IRepositories.WorkOrder;

namespace Lzq.WorkOrder.Application.Services.WorkOrder;

/// <summary>
/// 工单统计服务实现（供Dashboard模块使用）
/// </summary>
public class WorkOrderStatisticsService : ServiceBase, IWorkOrderStatisticsService
{
    public WorkOrderStatisticsService() : base("/api/v1/mes/work-order/statistics")
    {
    }

    private IWorkOrderRepository WorkOrderRepo => GetRequiredService<IWorkOrderRepository>();
    private IWorkReportRepository WorkReportRepo => GetRequiredService<IWorkReportRepository>();

    /// <inheritdoc/>
    public async Task<WorkOrderStatisticsResultDto> GetStatisticsAsync(DateTime start, DateTime end, long? lineId = null)
    {
        var query = WorkOrderRepo.AsQueryable()
            .Where(w => w.ActualStart >= start && w.ActualStart < end);

        if (lineId.HasValue)
        {
            query = query.Where(w => w.LineId == lineId.Value);
        }

        var totalCount = await query.CountAsync();
        if (totalCount == 0)
        {
            return new WorkOrderStatisticsResultDto
            {
                TotalCount = 0,
                CompletedCount = 0,
                InProgressCount = 0,
                PendingCount = 0,
                PausedCount = 0,
                CancelledCount = 0,
                CompletionRate = 0
            };
        }

        var completedCount = await query
            .Where(w => w.Status == WorkOrderStatusEnum.Completed || w.Status == WorkOrderStatusEnum.Closed)
            .CountAsync();

        var inProgressCount = await query
            .Where(w => w.Status == WorkOrderStatusEnum.InProgress || w.Status == WorkOrderStatusEnum.Dispatched)
            .CountAsync();

        var pausedCount = await query
            .Where(w => w.Status == WorkOrderStatusEnum.Paused)
            .CountAsync();

        var cancelledCount = await query
            .Where(w => w.Status == WorkOrderStatusEnum.Cancelled)
            .CountAsync();

        var pendingCount = totalCount - completedCount - inProgressCount - pausedCount - cancelledCount;

        return new WorkOrderStatisticsResultDto
        {
            TotalCount = totalCount,
            CompletedCount = completedCount,
            InProgressCount = inProgressCount,
            PendingCount = Math.Max(0, pendingCount),
            PausedCount = pausedCount,
            CancelledCount = cancelledCount,
            CompletionRate = Math.Round((decimal)completedCount / totalCount * 100, 2)
        };
    }

    /// <inheritdoc/>
    public async Task<int> GetProductionOutputAsync(DateTime start, DateTime end, long? lineId = null)
    {
        var query = WorkReportRepo.AsQueryable()
            .Where(w => w.ReportTime >= start && w.ReportTime < end);

        if (lineId.HasValue)
        {
            var workOrderIds = await WorkOrderRepo.AsQueryable()
                .Where(w => w.LineId == lineId.Value)
                .Select(w => w.Id)
                .ToListAsync();

            query = query.Where(w => workOrderIds.Contains(w.WorkOrderId));
        }

        return await query.SumAsync(w => w.QualifiedQty);
    }

    /// <inheritdoc/>
    public async Task<decimal> GetCompletionRateAsync(DateTime start, DateTime end, long? lineId = null)
    {
        var query = WorkOrderRepo.AsQueryable()
            .Where(w => w.ActualStart >= start && w.ActualStart < end);

        if (lineId.HasValue)
        {
            query = query.Where(w => w.LineId == lineId.Value);
        }

        var total = await query.CountAsync();
        if (total == 0) return 0;

        var completed = await query
            .Where(w => w.Status == WorkOrderStatusEnum.Completed || w.Status == WorkOrderStatusEnum.Closed)
            .CountAsync();

        return Math.Round((decimal)completed / total * 100, 2);
    }
}
