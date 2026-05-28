using Lzq.MES.Application.Contracts.IServices;
using Lzq.MES.Application.Contracts.Queries;
using Lzq.MES.Domain.Enums;
using Lzq.Extensions.AI.AgentSkills;
using Microsoft.Agents.AI;
using System.ComponentModel;
using System.Text.Json;

namespace Lzq.AgentSkills;

/// <summary>
/// 设备状态查询技能 —— 提供设备实时状态、故障历史、保养计划查询能力
/// </summary>
[GeneralSkill]
public class EquipmentStatusSkill : LzqAgentSkillBase<EquipmentStatusSkill>
{
    private readonly IEquipmentService _equipmentService;
    private readonly IEquipmentStatisticsService _statisticsService;
    private readonly IRepairOrderService _repairOrderService;
    private readonly IMaintenanceService _maintenanceService;

    public EquipmentStatusSkill(
        IEquipmentService equipmentService,
        IEquipmentStatisticsService statisticsService,
        IRepairOrderService repairOrderService,
        IMaintenanceService maintenanceService)
    {
        _equipmentService = equipmentService;
        _statisticsService = statisticsService;
        _repairOrderService = repairOrderService;
        _maintenanceService = maintenanceService;
    }

    // ==================== L1：元数据 ====================
    protected override string SkillName => "equipment-status";
    protected override string SkillDescription => "提供设备实时状态、运行数据、故障历史、保养计划查询能力。";

    // ==================== L2：执行指令 ====================
    protected override string Instructions => """
        你是一个设备管理专家。当用户询问设备相关问题时，请按以下指引操作：

        1. 若用户查询某台设备状态（如「注塑机A-01现在什么状态」），调用 GetStatus 脚本。
        2. 若用户要求查看设备概览（如「设备整体情况怎么样」），调用 GetOverview 脚本。
        3. 若用户询问故障或报修（如「有哪些设备在维修」），调用 ListFaults 脚本。
        4. 若用户询问保养计划（如「最近有什么保养安排」），调用 GetUpcomingMaintenance 脚本。
        5. 展示结果时，结合 equipment-status-rules 资源中的状态说明。
        """;

    // ==================== L4：业务规则资源 ====================
    [AgentSkillResource("equipment-status-rules")]
    [Description("设备状态、维修状态、保养类型说明")]
    public static string EquipmentRules => """
        ## 设备状态
        | 状态 | 含义 |
        |------|------|
        | Normal | 正常运行 |
        | UnderRepair | 维修中 |
        | UnderMaintenance | 保养中 |
        | Stopped | 停机 |
        | Scrapped | 已报废 |

        ## 维修状态
        | 状态 | 含义 |
        |------|------|
        | Pending | 待派工 |
        | Assigned | 已派工 |
        | InProgress | 维修中 |
        | Completed | 已完工待验收 |
        | Accepted | 已验收 |
        | Cancelled | 已取消 |

        ## 维修优先级
        | 优先级 | 含义 |
        |--------|------|
        | Urgent | 紧急 |
        | High | 高 |
        | Medium | 中 |
        | Low | 低 |

        ## 保养类型
        | 类型 | 含义 |
        |------|------|
        | Daily | 日常保养 |
        | Level1 | 一级保养 |
        | Level2 | 二级保养 |
        | Level3 | 三级保养 |
        | Precision | 精度保养 |
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

    [AgentSkillScript("GetStatus")]
    [Description("查询指定设备的详细状态信息。参数 equipmentCode 为设备编号。")]
    public async Task<string> GetStatusAsync(
        [Description("设备编号")] string equipmentCode)
    {
        try
        {
            var result = await _equipmentService.PageAsync(new EquipmentPageQuery
            {
                Code = equipmentCode,
                Page = 1,
                PageSize = 1,
            });

            if (result?.Data?.Items is null || result.Data.Items.Count == 0)
            {
                return $"未找到设备编号为 {equipmentCode} 的设备，请检查编号是否正确。";
            }

            var eq = result.Data.Items[0];

            var data = new
            {
                eq.Code,
                eq.Name,
                Type = eq.EquipmentTypeName ?? eq.EquipmentType.ToString(),
                eq.Spec,
                eq.Brand,
                Status = eq.StatusName ?? eq.Status.ToString(),
                Line = eq.LineName,
                eq.Location,
                Responsible = eq.ResponsibleName,
                RunningHours = eq.TotalRunningHours,
                RepairCount = eq.TotalRepairCount,
                eq.Remark,
                eq.CreateTime,
            };

            return JsonSerializer.Serialize(data, _jsonOptions);
        }
        catch (Exception ex)
        {
            return ErrorJson($"查询设备状态失败：{ex.Message}");
        }
    }

    [AgentSkillScript("GetOverview")]
    [Description("获取所有设备的整体状态概览和按产线分布。")]
    public async Task<string> GetOverviewAsync()
    {
        try
        {
            var overview = await _statisticsService.GetStatusOverviewAsync();
            var byLine = await _statisticsService.GetStatusByLineAsync();

            var data = new
            {
                Overview = overview is null ? null : new
                {
                    overview.TotalCount,
                    overview.NormalCount,
                    overview.UnderRepairCount,
                    overview.UnderMaintenanceCount,
                    overview.StoppedCount,
                    NormalRate = Math.Round(overview.NormalRate, 1),
                },
                ByLine = byLine?.Select(l => new
                {
                    Line = l.LineName ?? "未分配",
                    l.TotalCount,
                    l.NormalCount,
                    l.AbnormalCount,
                    NormalRate = Math.Round(l.NormalRate, 1),
                }).ToList(),
            };

            return JsonSerializer.Serialize(data, _jsonOptions);
        }
        catch (Exception ex)
        {
            return ErrorJson($"查询设备概览失败：{ex.Message}");
        }
    }

    [AgentSkillScript("ListFaults")]
    [Description("查询设备报修/故障列表，可按优先级和状态筛选。")]
    public async Task<string> ListFaultsAsync(
        [Description("优先级筛选：Urgent/High/Medium/Low，不传返回全部")] string? priority = null,
        [Description("维修状态筛选：Pending/Assigned/InProgress/Completed/Accepted/Cancelled，默认查询未完成的")] string? status = null)
    {
        try
        {
            Enum.TryParse<RepairPriorityEnum>(priority, true, out var parsedPriority);
            Enum.TryParse<RepairStatusEnum>(status, true, out var parsedStatus);
            var query = new RepairOrderPageQuery
            {
                Page = 1,
                PageSize = 50,
                Priority = !string.IsNullOrWhiteSpace(priority) ? parsedPriority : null,
                Status = !string.IsNullOrWhiteSpace(status) ? parsedStatus : null,
            };

            var result = await _repairOrderService.PageAsync(query);
            var items = result?.Data?.Items ?? [];

            if (string.IsNullOrWhiteSpace(status))
            {
                items = items.Where(r =>
                    r.Status != RepairStatusEnum.Accepted &&
                    r.Status != RepairStatusEnum.Cancelled).ToList();
            }

            var list = items.Select(r => new
            {
                r.Code,
                EquipmentCode = r.EquipmentCode,
                EquipmentName = r.EquipmentName,
                FaultDescription = r.Description,
                Priority = r.PriorityName ?? r.Priority.ToString(),
                Status = r.StatusName ?? r.Status.ToString(),
                Reporter = r.ReporterName,
                RepairUser = r.RepairUserName,
                r.ReportTime,
                r.RepairStartTime,
                r.RepairEndTime,
                r.FaultReason,
                r.WorkHours,
                r.Cost,
            }).ToList();

            return JsonSerializer.Serialize(new
            {
                Total = list.Count,
                Faults = list,
            }, _jsonOptions);
        }
        catch (Exception ex)
        {
            return ErrorJson($"查询故障列表失败：{ex.Message}");
        }
    }

    [AgentSkillScript("GetUpcomingMaintenance")]
    [Description("查询即将执行的保养计划（未来7天内）。")]
    public async Task<string> GetUpcomingMaintenanceAsync()
    {
        try
        {
            var today = DateTime.Today;
            var nextWeek = today.AddDays(7);

            var result = await _maintenanceService.PlanPageAsync(new MaintenancePlanPageQuery
            {
                Page = 1,
                PageSize = 50,
                PlanDateFrom = today,
                PlanDateTo = nextWeek,
            });

            var items = result?.Data?.Items ?? [];

            items = items.Where(p =>
                p.Status == MaintenancePlanStatusEnum.Pending ||
                p.Status == MaintenancePlanStatusEnum.Delayed).ToList();

            var list = items.Select(p => new
            {
                p.Code,
                p.Name,
                Equipment = p.EquipmentName,
                EquipmentCode = p.EquipmentCode,
                Type = p.MaintenanceTypeName ?? p.MaintenanceType.ToString(),
                p.PlanDate,
                Status = p.StatusName ?? p.Status.ToString(),
                Responsible = p.ResponsibleName,
                p.DurationMinutes,
                p.Content,
            }).ToList();

            return JsonSerializer.Serialize(new
            {
                Period = $"{today:yyyy-MM-dd} ~ {nextWeek:yyyy-MM-dd}",
                Total = list.Count,
                Plans = list,
            }, _jsonOptions);
        }
        catch (Exception ex)
        {
            return ErrorJson($"查询保养计划失败：{ex.Message}");
        }
    }
}
