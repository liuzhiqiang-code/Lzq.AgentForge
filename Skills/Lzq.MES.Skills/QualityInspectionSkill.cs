using Lzq.Extensions.AI.AgentSkills;
using Lzq.QA.Application.Contracts.IServices;
using Lzq.QA.Application.Contracts.Queries;
using Microsoft.Agents.AI;
using System.ComponentModel;
using System.Text.Json;

namespace Lzq.AgentSkills;

/// <summary>
/// 质量检验查询技能 —— 提供质检记录、不良品分析、检验统计查询能力
/// </summary>
[GeneralSkill]
public class QualityInspectionSkill : LzqAgentSkillBase<QualityInspectionSkill>
{
    private readonly IQCOrderService _qcOrderService;
    private readonly IQCStatisticsService _qcStatisticsService;
    private readonly IDefectRecordService _defectRecordService;

    public QualityInspectionSkill(
        IQCOrderService qcOrderService,
        IQCStatisticsService qcStatisticsService,
        IDefectRecordService defectRecordService)
    {
        _qcOrderService = qcOrderService;
        _qcStatisticsService = qcStatisticsService;
        _defectRecordService = defectRecordService;
    }

    // ==================== L1：元数据 ====================
    protected override string SkillName => "quality-inspection";
    protected override string SkillDescription => "提供质量检验记录、不良品分析、检验统计、缺陷追踪查询能力。";

    // ==================== L2：执行指令 ====================
    protected override string Instructions => """
        你是一个质量管理专家。当用户询问质检相关问题时，请按以下指引操作：

        1. 若用户提供质检单号，调用 GetInspectionResult 脚本查询该质检单详情。
        2. 若用户询问不良率统计（如「本月不良率是多少」），调用 GetDefectStats 脚本。
        3. 若用户要求列出不良品（如「今天有哪些不良品」），调用 ListDefects 脚本。
        4. 若用户询问缺陷汇总（如「缺陷怎么分布的」），调用 GetDefectSummary 脚本。
        5. 展示结果时，结合 quality-rules 资源中的质检类型和缺陷处理方式说明。
        """;

    // ==================== L4：业务规则资源 ====================
    [AgentSkillResource("quality-rules")]
    [Description("质检类型、判定结果、缺陷处理方式说明")]
    public static string QualityRules => """
        ## 质检类型
        | 类型 | 含义 | 说明 |
        |------|------|------|
        | IQC | 来料检验 | 原材料/外购件入库前检验 |
        | PQC | 过程检验 | 生产过程中的工序检验 |
        | OQC | 出货检验 | 成品出货前的最终检验 |

        ## 质检单状态
        | 状态 | 含义 |
        |------|------|
        | Pending | 待检验 |
        | InProgress | 检验中 |
        | Qualified | 合格 |
        | Unqualified | 不合格 |
        | Processed | 已处理 |
        | Cancelled | 已取消 |

        ## 检验结果
        | 结果 | 含义 |
        |------|------|
        | Pass | 合格 |
        | Fail | 不合格 |
        | AcceptWithRestriction | 让步接收 |

        ## 缺陷处理方式
        | 方式 | 含义 |
        |------|------|
        | Rework | 返工 |
        | Scrap | 报废 |
        | Downgrade | 降级使用 |
        | Return | 退货 |
        | AcceptSpecial | 特采 |
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

    [AgentSkillScript("GetInspectionResult")]
    [Description("通过质检单号查询质检详情和检验结果。")]
    public async Task<string> GetInspectionResultAsync(
        [Description("质检单编号")] string qcOrderCode)
    {
        try
        {
            var result = await _qcOrderService.PageAsync(new QCOrderPageQuery
            {
                Code = qcOrderCode,
                Page = 1,
                PageSize = 1,
            });

            if (result?.Data?.Items is null || result.Data.Items.Count == 0)
            {
                return $"未找到质检单 {qcOrderCode}，请检查编号是否正确。";
            }

            var qc = result.Data.Items[0];

            var itemsResult = await _qcOrderService.GetItemsAsync(qc.Id);
            var items = itemsResult?.Data?
                .Select(i => new
                {
                    i.ItemName,
                    Type = i.ItemTypeName ?? i.ItemType.ToString(),
                    i.Standard,
                    i.SampleQty,
                    i.QualifiedQty,
                    i.UnqualifiedQty,
                    Result = i.ResultName ?? i.Result.ToString(),
                    i.DefectDesc,
                    i.DefectCode,
                }).ToList() ?? [];

            var data = new
            {
                qc.Code,
                QCType = qc.QCTypeName ?? qc.QCType.ToString(),
                Product = qc.ProductName,
                Spec = qc.ProductSpec,
                BatchNo = qc.BatchNo,
                Supplier = qc.SupplierName,
                RefCode = qc.RefCode,
                SubmitQty = qc.SubmitQty,
                QualifiedQty = qc.QualifiedQty,
                UnqualifiedQty = qc.UnqualifiedQty,
                Status = qc.StatusName ?? qc.Status.ToString(),
                Inspector = qc.InspectorName,
                InspectDate = qc.InspectDate,
                CompletedTime = qc.CompletedTime,
                QCStandard = qc.QCStandard,
                Conclusion = qc.Conclusion,
                Items = items,
                qc.CreateTime,
            };

            return JsonSerializer.Serialize(data, _jsonOptions);
        }
        catch (Exception ex)
        {
            return ErrorJson($"查询质检结果失败：{ex.Message}");
        }
    }

    [AgentSkillScript("GetDefectStats")]
    [Description("获取指定时间范围内的不良率统计。")]
    public async Task<string> GetDefectStatsAsync(
        [Description("开始日期，格式：yyyy-MM-dd")] string startDate,
        [Description("结束日期，格式：yyyy-MM-dd")] string endDate)
    {
        try
        {
            var start = DateTime.Parse(startDate).Date;
            var end = DateTime.Parse(endDate).Date.AddDays(1).AddSeconds(-1);

            var stats = await _qcStatisticsService.GetDefectRateStatisticsAsync(start, end);

            var data = new
            {
                Period = new { Start = start.ToString("yyyy-MM-dd"), End = end.ToString("yyyy-MM-dd") },
                SubmitQty = stats.SubmitQty,
                QualifiedQty = stats.QualifiedQty,
                UnqualifiedQty = stats.UnqualifiedQty,
                QualifiedRate = Math.Round(stats.QualifiedRate, 2),
                DefectRate = Math.Round(stats.DefectRate, 2),
            };

            return JsonSerializer.Serialize(data, _jsonOptions);
        }
        catch (Exception ex)
        {
            return ErrorJson($"查询不良率统计失败：{ex.Message}");
        }
    }

    [AgentSkillScript("ListDefects")]
    [Description("查询不良品记录列表，可按批次号、缺陷代码、时间范围筛选。")]
    public async Task<string> ListDefectsAsync(
        [Description("批次号筛选")] string? batchNo = null,
        [Description("缺陷代码筛选")] string? defectCode = null,
        [Description("查询最近N天，默认7天")] int days = 7)
    {
        try
        {
            var query = new DefectRecordPageQuery
            {
                Page = 1,
                PageSize = 100,
                CreateTimeFrom = DateTime.Today.AddDays(-days),
                CreateTimeTo = DateTime.Today.AddDays(1).AddSeconds(-1),
                BatchNo = !string.IsNullOrWhiteSpace(batchNo) ? batchNo : null,
                DefectCode = !string.IsNullOrWhiteSpace(defectCode) ? defectCode : null,
            };

            var result = await _defectRecordService.PageAsync(query);
            var items = result?.Data?.Items ?? [];

            var list = items.Select(d => new
            {
                d.Id,
                QCOrderCode = d.QCOrderCode,
                WorkOrderCode = d.WorkOrderCode,
                Product = d.ProductName,
                Spec = d.ProductSpec,
                d.BatchNo,
                d.DefectQty,
                d.DefectCode,
                d.DefectDesc,
                Status = d.StatusName ?? d.Status.ToString(),
                Handling = d.HandlingTypeName ?? d.HandlingType?.ToString(),
                Handler = d.HandlerName,
                d.HandlingTime,
                d.CreateTime,
            }).ToList();

            var defectGroups = list
                .GroupBy(d => d.DefectCode)
                .Select(g => new
                {
                    DefectCode = g.Key,
                    DefectDesc = g.First().DefectDesc,
                    Count = g.Count(),
                    TotalQty = g.Sum(d => d.DefectQty),
                })
                .OrderByDescending(g => g.TotalQty)
                .ToList();

            var data = new
            {
                Period = $"最近{days}天",
                Total = list.Count,
                TotalDefectQty = list.Sum(d => d.DefectQty),
                DefectGroups = defectGroups,
                Records = list,
            };

            return JsonSerializer.Serialize(data, _jsonOptions);
        }
        catch (Exception ex)
        {
            return ErrorJson($"查询缺陷列表失败：{ex.Message}");
        }
    }

    [AgentSkillScript("GetDefectSummary")]
    [Description("获取指定时间范围内的缺陷处理汇总统计。")]
    public async Task<string> GetDefectSummaryAsync(
        [Description("开始日期，格式：yyyy-MM-dd")] string startDate,
        [Description("结束日期，格式：yyyy-MM-dd")] string endDate)
    {
        try
        {
            var start = DateTime.Parse(startDate).Date;
            var end = DateTime.Parse(endDate).Date.AddDays(1).AddSeconds(-1);

            var result = await _defectRecordService.GetStatisticsAsync(start, end);
            var stats = result?.Data;

            if (stats is null)
            {
                return JsonSerializer.Serialize(new { Message = "暂无缺陷数据" }, _jsonOptions);
            }

            var data = new
            {
                Period = new { Start = start.ToString("yyyy-MM-dd"), End = end.ToString("yyyy-MM-dd") },
                TotalDefectCount = stats.TotalDefectCount,
                ByHandling = new
                {
                    Rework = stats.ReworkCount,
                    Scrap = stats.ScrapCount,
                    Downgrade = stats.DowngradeCount,
                    Return = stats.ReturnCount,
                },
                ByStatus = new
                {
                    Pending = stats.PendingCount,
                    Processing = stats.ProcessingCount,
                    Processed = stats.ProcessedCount,
                },
            };

            return JsonSerializer.Serialize(data, _jsonOptions);
        }
        catch (Exception ex)
        {
            return ErrorJson($"查询缺陷汇总失败：{ex.Message}");
        }
    }
}
