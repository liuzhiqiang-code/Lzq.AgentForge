using Lzq.Extensions.AI.AgentSkills;
using Lzq.MES.Application.Contracts.IServices;
using Lzq.MES.Application.Contracts.Queries;
using Lzq.MES.Application.Contracts.IServices.WorkOrder;
using Lzq.MES.Application.Contracts.WorkOrder.Queries;
using Microsoft.Agents.AI;
using System.ComponentModel;
using System.Text.Json;

namespace Lzq.AgentSkills;

/// <summary>
/// 追溯查询技能 —— 提供产品全生命周期追溯（工单→质检→缺陷）查询能力
/// </summary>
[GeneralSkill]
public class TraceabilitySkill : LzqAgentSkillBase<TraceabilitySkill>
{
    private readonly IWorkOrderService _workOrderService;
    private readonly IQCOrderService _qcOrderService;
    private readonly IDefectRecordService _defectRecordService;

    public TraceabilitySkill(
        IWorkOrderService workOrderService,
        IQCOrderService qcOrderService,
        IDefectRecordService defectRecordService)
    {
        _workOrderService = workOrderService;
        _qcOrderService = qcOrderService;
        _defectRecordService = defectRecordService;
    }

    // ==================== L1：元数据 ====================
    protected override string SkillName => "traceability";
    protected override string SkillDescription => "提供产品全生命周期追溯查询能力，关联工单→质检→缺陷记录。";

    // ==================== L2：执行指令 ====================
    protected override string Instructions => """
        你是一个质量追溯专家。当用户需要追溯产品信息时，请按以下指引操作：

        1. 若用户提供工单号要求完整追溯，调用 GetFullTrace 脚本。
        2. 若用户只关心缺陷记录，调用 GetDefectsByWorkOrder 脚本。
        3. 若用户只关心质检记录，调用 GetQCByWorkOrder 脚本。
        4. 展示结果时，结合 traceability-rules 资源说明追溯维度和数据关联关系。
        """;

    // ==================== L4：业务规则资源 ====================
    [AgentSkillResource("traceability-rules")]
    [Description("追溯维度与数据关联说明")]
    public static string TraceabilityRules => """
        ## 追溯维度
        MES 系统支持以下追溯链路：

        ### 正向追溯：原材料 → 成品
        原材料批次 → 工单 → 报工记录 → 质检记录 → 成品入库

        ### 反向追溯：成品 → 原材料
        成品/工单 → 质检记录 → 缺陷记录 → 缺陷处理

        ## 当前技能支持范围
        - 工单 → 质检单：通过 RefCode（质检单关联工单号）关联
        - 工单 → 缺陷记录：通过 WorkOrderCode 关联
        - 质检单 → 缺陷记录：通过 QCOrderCode 关联

        ## 说明
        - 仅支持已接入 MES 系统的数据追溯
        - 原材料批次追溯需要物料模块支持（规划中）
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

    [AgentSkillScript("GetFullTrace")]
    [Description("通过工单号获取完整追溯信息，包括工单详情、关联质检单和缺陷记录。")]
    public async Task<string> GetFullTraceAsync(
        [Description("工单编号")] string workOrderCode)
    {
        try
        {
            var woResult = await _workOrderService.PageAsync(new WorkOrderPageQuery
            {
                Code = workOrderCode,
                Page = 1,
                PageSize = 1,
            });

            if (woResult?.Data?.Items is null || woResult.Data.Items.Count == 0)
            {
                return $"未找到工单 {workOrderCode}，请检查工单编号是否正确。";
            }

            var wo = woResult.Data.Items[0];

            var qcResult = await _qcOrderService.PageAsync(new QCOrderPageQuery
            {
                RefCode = workOrderCode,
                Page = 1,
                PageSize = 50,
            });

            var qcOrders = qcResult?.Data?.Items?
                .Select(qc => new
                {
                    qc.Code,
                    QCType = qc.QCTypeName ?? qc.QCType.ToString(),
                    qc.SubmitQty,
                    qc.QualifiedQty,
                    qc.UnqualifiedQty,
                    Status = qc.StatusName ?? qc.Status.ToString(),
                    Inspector = qc.InspectorName,
                    qc.InspectDate,
                    qc.Conclusion,
                }).ToList() ?? [];

            var defectResult = await _defectRecordService.PageAsync(new DefectRecordPageQuery
            {
                WorkOrderCode = workOrderCode,
                Page = 1,
                PageSize = 100,
            });

            var defects = defectResult?.Data?.Items?
                .Select(d => new
                {
                    d.Id,
                    QCOrderCode = d.QCOrderCode,
                    Product = d.ProductName,
                    d.BatchNo,
                    d.DefectQty,
                    d.DefectCode,
                    d.DefectDesc,
                    Status = d.StatusName ?? d.Status.ToString(),
                    Handling = d.HandlingTypeName ?? d.HandlingType?.ToString(),
                    Handler = d.HandlerName,
                    d.HandlingTime,
                    d.HandlingRemark,
                    d.CreateTime,
                }).ToList() ?? [];

            var totalDefectQty = defects.Sum(d => d.DefectQty);

            var data = new
            {
                WorkOrder = new
                {
                    wo.Code,
                    Product = wo.ProductName,
                    Spec = wo.ProductSpec,
                    Line = wo.LineName,
                    Process = wo.ProcessName,
                    PlannedQty = wo.PlannedQty,
                    CompletedQty = wo.CompletedQty,
                    DefectQtyInWO = wo.DefectQty,
                    Status = wo.StatusName ?? wo.Status.ToString(),
                    Priority = wo.Priority,
                    Planned = new { wo.PlannedStart, wo.PlannedEnd },
                    Actual = new { wo.ActualStart, wo.ActualEnd },
                    wo.CreateTime,
                    wo.UpdateTime,
                },
                QualityInspections = new
                {
                    Total = qcOrders.Count,
                    Orders = qcOrders,
                },
                DefectRecords = new
                {
                    Total = defects.Count,
                    TotalDefectQty = totalDefectQty,
                    Records = defects,
                },
                Summary = new
                {
                    PlannedQty = wo.PlannedQty,
                    QualifiedOutput = wo.CompletedQty - wo.DefectQty,
                    DefectInWO = wo.DefectQty,
                    DefectFoundInQC = totalDefectQty,
                    OverallQualifiedRate = wo.PlannedQty > 0
                        ? Math.Round((decimal)(wo.CompletedQty - wo.DefectQty) / wo.PlannedQty * 100, 2)
                        : 0m,
                },
            };

            return JsonSerializer.Serialize(data, _jsonOptions);
        }
        catch (Exception ex)
        {
            return ErrorJson($"完整追溯查询失败：{ex.Message}");
        }
    }

    [AgentSkillScript("GetDefectsByWorkOrder")]
    [Description("查询指定工单关联的所有缺陷记录。")]
    public async Task<string> GetDefectsByWorkOrderAsync(
        [Description("工单编号")] string workOrderCode)
    {
        try
        {
            var result = await _defectRecordService.PageAsync(new DefectRecordPageQuery
            {
                WorkOrderCode = workOrderCode,
                Page = 1,
                PageSize = 100,
            });

            var items = result?.Data?.Items ?? [];

            var list = items.Select(d => new
            {
                d.Id,
                QCOrderCode = d.QCOrderCode,
                Product = d.ProductName,
                d.BatchNo,
                d.DefectQty,
                d.DefectCode,
                d.DefectDesc,
                Status = d.StatusName ?? d.Status.ToString(),
                Handling = d.HandlingTypeName ?? d.HandlingType?.ToString(),
                Handler = d.HandlerName,
                d.HandlingRemark,
                d.CreateTime,
            }).ToList();

            var data = new
            {
                WorkOrderCode = workOrderCode,
                TotalDefects = list.Count,
                TotalDefectQty = list.Sum(d => d.DefectQty),
                Defects = list,
            };

            return JsonSerializer.Serialize(data, _jsonOptions);
        }
        catch (Exception ex)
        {
            return ErrorJson($"查询工单缺陷记录失败：{ex.Message}");
        }
    }

    [AgentSkillScript("GetQCByWorkOrder")]
    [Description("查询指定工单关联的所有质检单记录。")]
    public async Task<string> GetQCByWorkOrderAsync(
        [Description("工单编号")] string workOrderCode)
    {
        try
        {
            var result = await _qcOrderService.PageAsync(new QCOrderPageQuery
            {
                RefCode = workOrderCode,
                Page = 1,
                PageSize = 50,
            });

            var items = result?.Data?.Items ?? [];

            var list = items.Select(qc => new
            {
                qc.Code,
                QCType = qc.QCTypeName ?? qc.QCType.ToString(),
                Product = qc.ProductName,
                Spec = qc.ProductSpec,
                BatchNo = qc.BatchNo,
                Supplier = qc.SupplierName,
                SubmitQty = qc.SubmitQty,
                QualifiedQty = qc.QualifiedQty,
                UnqualifiedQty = qc.UnqualifiedQty,
                Status = qc.StatusName ?? qc.Status.ToString(),
                Inspector = qc.InspectorName,
                qc.InspectDate,
                qc.Conclusion,
                qc.CreateTime,
            }).ToList();

            var data = new
            {
                WorkOrderCode = workOrderCode,
                Total = list.Count,
                QCOrders = list,
            };

            return JsonSerializer.Serialize(data, _jsonOptions);
        }
        catch (Exception ex)
        {
            return ErrorJson($"查询工单质检记录失败：{ex.Message}");
        }
    }
}
