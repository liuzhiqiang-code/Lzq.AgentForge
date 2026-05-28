using Lzq.Extensions.AI.AgentSkills;
using Lzq.MES.Application.Contracts.IServices.WorkOrder;
using Lzq.MES.Application.Contracts.WorkOrder.Queries;
using Lzq.MES.Domain.Enums;
using Microsoft.Agents.AI;
using System.ComponentModel;
using System.Text.Json;

namespace Lzq.AgentSkills;

/// <summary>
/// 工单查询技能 —— 提供工单详情、列表查询、生产进度统计能力
/// </summary>
[GeneralSkill]
public class QueryWorkOrderSkill : LzqAgentSkillBase<QueryWorkOrderSkill>
{
    private readonly IWorkOrderService _workOrderService;
    private readonly IWorkOrderStatisticsService _statisticsService;

    public QueryWorkOrderSkill(
        IWorkOrderService workOrderService,
        IWorkOrderStatisticsService statisticsService)
    {
        _workOrderService = workOrderService;
        _statisticsService = statisticsService;
    }

    // ==================== L1：元数据 ====================
    protected override string SkillName => "work-order-query";
    protected override string SkillDescription => "提供生产系统工单详情、实时生产进度查询、工单列表检索能力。";

    // ==================== L2：执行指令 ====================
    protected override string Instructions => """
        你是一个工单管理专家。当用户询问工单相关问题时，请按以下指引操作：

        1. 若用户提供工单号，调用 GetProgress 脚本查询该工单详情与进度。
        2. 若用户要求列出全部工单或按状态筛选（如「有哪些待处理的工单」），调用 ListOrders 脚本获取列表。
        3. 若用户询问工单统计概览（如「本月完成了多少工单」），调用 GetStatistics 脚本获取统计数据。
        4. 展示结果时，结合 work-order-rules 资源中的状态说明，用通俗语言解释当前状态含义。
        """;

    // ==================== L4：业务规则资源 ====================
    [AgentSkillResource("work-order-rules")]
    [Description("工单状态和流转规则说明")]
    public static string WorkOrderRules => """
        ## 工单状态说明
        | 状态 | 含义 | 说明 |
        |------|------|------|
        | Draft (草稿) | 工单已创建，尚未派发 | 可编辑、删除 |
        | Dispatched (已派发) | 已派发到产线/人员 | 可开工或取消 |
        | InProgress (生产中) | 正在执行生产 | 可报工、暂停、完工 |
        | Completed (已完成) | 生产执行完毕 | 可关闭 |
        | Closed (已关闭) | 工单归档 | 不可再操作 |
        | Paused (已暂停) | 临时中断 | 可恢复生产 |
        | Cancelled (已取消) | 工单作废 | 不可恢复 |

        ## 工单号格式
        - 格式由系统自动生成，可通过工单号精确查询
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

    [AgentSkillScript("GetProgress")]
    [Description("通过工单号查询工单详情与生产进度。参数 workOrderCode 为工单编号。")]
    public async Task<string> GetProgressAsync(
        [Description("工单编号")] string workOrderCode)
    {
        try
        {
            var result = await _workOrderService.PageAsync(new WorkOrderPageQuery
            {
                Code = workOrderCode,
                Page = 1,
                PageSize = 1,
            });

            if (result?.Data?.Items is null || result.Data.Items.Count == 0)
            {
                return $"未找到工单 {workOrderCode}，请检查工单编号是否正确。";
            }

            var order = result.Data.Items[0];
            var progress = order.PlannedQty > 0
                ? Math.Round((decimal)order.CompletedQty / order.PlannedQty * 100, 1)
                : 0m;

            var data = new
            {
                order.Code,
                Product = order.ProductName,
                Spec = order.ProductSpec,
                Line = order.LineName,
                Process = order.ProcessName,
                PlannedQty = order.PlannedQty,
                CompletedQty = order.CompletedQty,
                DefectQty = order.DefectQty,
                ProgressPercent = progress,
                Status = order.StatusName ?? order.Status.ToString(),
                Priority = order.Priority,
                Planned = new { order.PlannedStart, order.PlannedEnd },
                Actual = new { order.ActualStart, order.ActualEnd },
                order.Remark,
                order.CreateTime,
            };

            return JsonSerializer.Serialize(data, _jsonOptions);
        }
        catch (Exception ex)
        {
            return ErrorJson($"查询工单进度失败：{ex.Message}");
        }
    }

    [AgentSkillScript("ListOrders")]
    [Description("获取工单列表，可按状态筛选。不传参数则返回最近工单。")]
    public async Task<string> ListOrdersAsync(
        [Description("状态筛选：Draft/Dispatched/InProgress/Completed/Closed/Paused/Cancelled，不传返回全部")] string? status = null,
        [Description("产线名称模糊筛选")] string? lineName = null,
        [Description("产品名称模糊筛选")] string? productName = null)
    {
        try
        {
            Enum.TryParse<WorkOrderStatusEnum>(status, true, out var parsedStatus);
            var query = new WorkOrderPageQuery
            {
                Page = 1,
                PageSize = 50,
                Status = !string.IsNullOrWhiteSpace(status) ? parsedStatus : null,
                ProductName = !string.IsNullOrWhiteSpace(productName) ? productName : null,
            };

            var result = await _workOrderService.PageAsync(query);
            var items = result?.Data?.Items ?? [];

            if (!string.IsNullOrWhiteSpace(lineName))
                items = items.Where(o => o.LineName?.Contains(lineName) ?? false).ToList();

            var list = items.Select(o => new
            {
                o.Code,
                Product = o.ProductName,
                o.ProductSpec,
                Line = o.LineName,
                Process = o.ProcessName,
                o.PlannedQty,
                o.CompletedQty,
                o.DefectQty,
                ProgressPercent = o.PlannedQty > 0
                    ? Math.Round((decimal)o.CompletedQty / o.PlannedQty * 100, 1)
                    : 0m,
                Status = o.StatusName ?? o.Status.ToString(),
                o.Priority,
                o.CreateTime,
            }).ToList();

            return JsonSerializer.Serialize(new
            {
                Total = list.Count,
                Orders = list,
            }, _jsonOptions);
        }
        catch (Exception ex)
        {
            return ErrorJson($"查询工单列表失败：{ex.Message}");
        }
    }

    [AgentSkillScript("GetStatistics")]
    [Description("获取指定时间范围内的工单统计概览（总数、完成率、产量等）。")]
    public async Task<string> GetStatisticsAsync(
        [Description("开始日期，格式：yyyy-MM-dd，默认本月1日")] string? startDate = null,
        [Description("结束日期，格式：yyyy-MM-dd，默认今天")] string? endDate = null)
    {
        try
        {
            var now = DateTime.Now;
            var start = string.IsNullOrWhiteSpace(startDate)
                ? new DateTime(now.Year, now.Month, 1)
                : DateTime.Parse(startDate);
            var end = string.IsNullOrWhiteSpace(endDate)
                ? now.Date.AddDays(1).AddSeconds(-1)
                : DateTime.Parse(endDate).Date.AddDays(1).AddSeconds(-1);

            var stats = await _statisticsService.GetStatisticsAsync(start, end);
            var outputQty = await _statisticsService.GetProductionOutputAsync(start, end);
            var completionRate = await _statisticsService.GetCompletionRateAsync(start, end);

            var data = new
            {
                Period = new { Start = start.ToString("yyyy-MM-dd"), End = end.ToString("yyyy-MM-dd") },
                TotalCount = stats.TotalCount,
                CompletedCount = stats.CompletedCount,
                InProgressCount = stats.InProgressCount,
                PendingCount = stats.PendingCount,
                PausedCount = stats.PausedCount,
                CancelledCount = stats.CancelledCount,
                CompletionRate = Math.Round(stats.CompletionRate, 1),
                TotalOutput = outputQty,
            };

            return JsonSerializer.Serialize(data, _jsonOptions);
        }
        catch (Exception ex)
        {
            return ErrorJson($"查询工单统计失败：{ex.Message}");
        }
    }
}
