using Lzq.Extensions.AI.AgentSkills;
using Lzq.WorkOrder.Application.Contracts.IServices.WorkOrder;
using Lzq.WorkOrder.Application.Contracts.WorkOrder.Queries;
using Lzq.WorkOrder.Domain.Enums;
using Microsoft.Agents.AI;
using System.ComponentModel;
using System.Text.Json;

namespace Lzq.AgentSkills;

/// <summary>
/// 生产进度跟踪技能 —— 提供生产进度、产线产能、产量统计查询能力
/// </summary>
[GeneralSkill]
public class ProductionProgressSkill : LzqAgentSkillBase<ProductionProgressSkill>
{
    private readonly IWorkOrderService _workOrderService;
    private readonly IWorkOrderStatisticsService _statisticsService;

    public ProductionProgressSkill(
        IWorkOrderService workOrderService,
        IWorkOrderStatisticsService statisticsService)
    {
        _workOrderService = workOrderService;
        _statisticsService = statisticsService;
    }

    // ==================== L1：元数据 ====================
    protected override string SkillName => "production-progress";
    protected override string SkillDescription => "提供生产进度、产能分析、工单完成情况和产量统计查询能力。";

    // ==================== L2：执行指令 ====================
    protected override string Instructions => """
        你是一个生产管理专家。当用户询问生产进度、产量、产能相关问题时，请按以下指引操作：

        1. 若用户询问当日/某日生产进度概览，调用 GetDailyProgress 脚本。
        2. 若用户询问某产线的生产情况，调用 GetLineProgress 脚本。
        3. 若用户询问完成率，调用 GetCompletionRate 脚本。
        4. 若用户询问产量，调用 GetOutput 脚本。
        5. 展示结果时，结合 production-rules 资源说明工单状态的含义。
        """;

    // ==================== L4：业务规则资源 ====================
    [AgentSkillResource("production-rules")]
    [Description("生产工单状态流转说明")]
    public static string ProductionRules => """
        ## 工单状态流转
        Draft(草稿) → Dispatched(已派发) → InProgress(生产中) → Completed(已完成) → Closed(已关闭)
        
        特殊流转：
        - InProgress → Paused(已暂停) → InProgress (恢复)
        - Dispatched → Cancelled(已取消)
        - InProgress → Cancelled(已取消)

        ## 产量计算
        - 计划产量 = 工单上设定的 PlannedQty
        - 已完成产量 = 报工累计的合格数
        - 不良品数 = 报工中标记的不合格数
        - 进度(%) = 已完成产量 / 计划产量 × 100
        """;

    // ==================== 辅助方法 ====================
    private static string ErrorJson(string message) => JsonSerializer.Serialize(
        new { Error = message },
        new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    // ==================== L3：脚本 ====================

    [AgentSkillScript("GetDailyProgress")]
    [Description("获取指定日期的生产进度总览。不传日期默认查询今天。")]
    public async Task<string> GetDailyProgressAsync(
        [Description("日期，格式：yyyy-MM-dd，默认今天")] string? date = null)
    {
        try
        {
            var targetDate = string.IsNullOrWhiteSpace(date)
                ? DateTime.Today
                : DateTime.Parse(date);

            var start = targetDate.Date;
            var end = targetDate.Date.AddDays(1).AddSeconds(-1);

            var stats = await _statisticsService.GetStatisticsAsync(start, end);
            var output = await _statisticsService.GetProductionOutputAsync(start, end);
            var rate = await _statisticsService.GetCompletionRateAsync(start, end);

            var inProgressResult = await _workOrderService.PageAsync(new WorkOrderPageQuery
            {
                Status = WorkOrderStatusEnum.InProgress,
                Page = 1,
                PageSize = 100,
            });

            var inProgressOrders = inProgressResult?.Data?.Items?
                .Select(o => new
                {
                    o.Code,
                    Product = o.ProductName,
                    Line = o.LineName,
                    o.PlannedQty,
                    o.CompletedQty,
                    o.DefectQty,
                    Progress = o.PlannedQty > 0
                        ? Math.Round((decimal)o.CompletedQty / o.PlannedQty * 100, 1)
                        : 0m,
                }).ToList() ?? [];

            var data = new
            {
                Date = targetDate.ToString("yyyy-MM-dd"),
                Statistics = new
                {
                    stats.TotalCount,
                    stats.CompletedCount,
                    stats.InProgressCount,
                    stats.PendingCount,
                    stats.PausedCount,
                    ProductionOutput = output,
                    CompletionRate = Math.Round(rate, 1),
                },
                InProgressOrders = inProgressOrders,
            };

            return JsonSerializer.Serialize(data, _jsonOptions);
        }
        catch (Exception ex)
        {
            return ErrorJson($"查询每日生产进度失败：{ex.Message}");
        }
    }

    [AgentSkillScript("GetLineProgress")]
    [Description("查询指定产线的生产进度，可按状态筛选。")]
    public async Task<string> GetLineProgressAsync(
        [Description("产线名称（模糊匹配）")] string lineName,
        [Description("状态筛选：Draft/Dispatched/InProgress/Completed/Paused，不传返回进行中的")] string? status = null)
    {
        try
        {
            Enum.TryParse<WorkOrderStatusEnum>(status, true, out var parsedStatus);
            var query = new WorkOrderPageQuery
            {
                Page = 1,
                PageSize = 100,
                Status = !string.IsNullOrWhiteSpace(status) ? parsedStatus : WorkOrderStatusEnum.InProgress,
            };

            var result = await _workOrderService.PageAsync(query);
            var items = result?.Data?.Items ?? [];

            items = items.Where(o => o.LineName?.Contains(lineName) ?? false).ToList();

            var totalPlanned = items.Sum(o => o.PlannedQty);
            var totalCompleted = items.Sum(o => o.CompletedQty);
            var totalDefect = items.Sum(o => o.DefectQty);

            var list = items.Select(o => new
            {
                o.Code,
                Product = o.ProductName,
                Process = o.ProcessName,
                o.PlannedQty,
                o.CompletedQty,
                o.DefectQty,
                Progress = o.PlannedQty > 0
                    ? Math.Round((decimal)o.CompletedQty / o.PlannedQty * 100, 1)
                    : 0m,
                Status = o.StatusName ?? o.Status.ToString(),
                o.Priority,
            }).ToList();

            var data = new
            {
                Line = lineName,
                Summary = new
                {
                    TotalOrders = list.Count,
                    TotalPlanned = totalPlanned,
                    TotalCompleted = totalCompleted,
                    TotalDefect = totalDefect,
                    OverallProgress = totalPlanned > 0
                        ? Math.Round((decimal)totalCompleted / totalPlanned * 100, 1)
                        : 0m,
                },
                Orders = list,
            };

            return JsonSerializer.Serialize(data, _jsonOptions);
        }
        catch (Exception ex)
        {
            return ErrorJson($"查询产线生产进度失败：{ex.Message}");
        }
    }

    [AgentSkillScript("GetCompletionRate")]
    [Description("获取指定时间范围内的工单完成率和产量。")]
    public async Task<string> GetCompletionRateAsync(
        [Description("开始日期，格式：yyyy-MM-dd")] string startDate,
        [Description("结束日期，格式：yyyy-MM-dd")] string endDate)
    {
        try
        {
            var start = DateTime.Parse(startDate).Date;
            var end = DateTime.Parse(endDate).Date.AddDays(1).AddSeconds(-1);

            var rate = await _statisticsService.GetCompletionRateAsync(start, end);
            var output = await _statisticsService.GetProductionOutputAsync(start, end);
            var stats = await _statisticsService.GetStatisticsAsync(start, end);

            var data = new
            {
                Period = new { Start = start.ToString("yyyy-MM-dd"), End = end.ToString("yyyy-MM-dd") },
                TotalOrders = stats.TotalCount,
                CompletedOrders = stats.CompletedCount,
                CompletionRate = Math.Round(rate, 1),
                TotalOutput = output,
            };

            return JsonSerializer.Serialize(data, _jsonOptions);
        }
        catch (Exception ex)
        {
            return ErrorJson($"查询完成率失败：{ex.Message}");
        }
    }

    [AgentSkillScript("GetOutput")]
    [Description("获取指定时间范围内的合格产量统计。")]
    public async Task<string> GetOutputAsync(
        [Description("开始日期，格式：yyyy-MM-dd，默认本月1日")] string? startDate = null,
        [Description("结束日期，格式：yyyy-MM-dd，默认今天")] string? endDate = null)
    {
        try
        {
            var now = DateTime.Now;
            var start = string.IsNullOrWhiteSpace(startDate)
                ? new DateTime(now.Year, now.Month, 1)
                : DateTime.Parse(startDate).Date;
            var end = string.IsNullOrWhiteSpace(endDate)
                ? now.Date.AddDays(1).AddSeconds(-1)
                : DateTime.Parse(endDate).Date.AddDays(1).AddSeconds(-1);

            var output = await _statisticsService.GetProductionOutputAsync(start, end);
            var rate = await _statisticsService.GetCompletionRateAsync(start, end);

            var data = new
            {
                Period = new { Start = start.ToString("yyyy-MM-dd"), End = end.ToString("yyyy-MM-dd") },
                QualifiedOutput = output,
                CompletionRate = Math.Round(rate, 1),
            };

            return JsonSerializer.Serialize(data, _jsonOptions);
        }
        catch (Exception ex)
        {
            return ErrorJson($"查询产量统计失败：{ex.Message}");
        }
    }
}
