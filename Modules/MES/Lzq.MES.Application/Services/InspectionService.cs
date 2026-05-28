using Microsoft.AspNetCore.Builder;
using Lzq.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Lzq.MES.Domain.Enums;
using Mapster;
using NSwag.Annotations;
using SqlSugar;
using Lzq.MES.Application.Contracts.Commands;
using Lzq.MES.Application.Contracts.Queries;
using Lzq.MES.Application.Contracts.IServices;
using Lzq.MES.Application.Contracts.Dtos;
using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.IRepositories;

namespace Lzq.MES.Application.Services;

/// <summary>
/// 点检服务
/// </summary>
public class InspectionService : ServiceBase, IInspectionService
{
    public InspectionService() : base("/api/v1/mes/inspection") { }

    private IInspectionPlanRepository PlanRepo => GetRequiredService<IInspectionPlanRepository>();
    private IInspectionRecordRepository RecordRepo => GetRequiredService<IInspectionRecordRepository>();
    private IInspectionItemRepository ItemRepo => GetRequiredService<IInspectionItemRepository>();
    private IEquipmentRepository EquipmentRepo => GetRequiredService<IEquipmentRepository>();
    private IRepairOrderRepository RepairRepo => GetRequiredService<IRepairOrderRepository>();

    #region 点检计划

    /// <summary>
    /// 点检计划分页查询
    /// </summary>
    [OpenApiTag("mes/inspection"), OpenApiOperation("点检计划分页查询", "支持按编号、名称、设备筛选")]
    [RoutePattern(pattern: "plan/page", true)]
    public async Task<ApiResult<PagedResponse<InspectionPlanViewDto>>> PlanPageAsync([FromBody] InspectionPlanPageQuery query)
    {
        var expr = PlanRepo.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(query.Code), p => p.Code!.Contains(query.Code!))
            .WhereIF(!string.IsNullOrEmpty(query.Name), p => p.Name!.Contains(query.Name!))
            .WhereIF(query.EquipmentId.HasValue, p => p.EquipmentId == query.EquipmentId.Value)
            .WhereIF(query.IsEnabled.HasValue, p => p.IsEnabled == query.IsEnabled.Value)
            .WhereIF(query.NextInspectDateFrom.HasValue, p => p.NextInspectDate >= query.NextInspectDateFrom.Value)
            .WhereIF(query.NextInspectDateTo.HasValue, p => p.NextInspectDate <= query.NextInspectDateTo.Value);

        RefAsync<int> total = 0;
        var list = await expr.OrderBy(p => p.NextInspectDate)
            .ToPageListAsync(query.Page, query.PageSize, total);

        var dtos = list.Select(p =>
        {
            var dto = p.Map<InspectionPlanViewDto>();
            dto.Code = $"DJC{p.Id:D8}";
            dto.CycleTypeName = GetCycleTypeName(p.CycleType);
            return dto;
        }).ToList();

        return ApiResult.Success(new PagedResponse<InspectionPlanViewDto>(dtos, total));
    }

    /// <summary>
    /// 获取点检计划详情
    /// </summary>
    [OpenApiTag("mes/inspection"), OpenApiOperation("获取点检计划详情", "根据ID获取点检计划详细信息")]
    [RoutePattern(pattern: "plan/{id}", true)]
    public async Task<ApiResult<InspectionPlanViewDto>> GetPlanAsync(long id)
    {
        var entity = await PlanRepo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("点检计划不存在");

        var dto = entity.Map<InspectionPlanViewDto>();
        dto.Code = $"DJC{entity.Id:D8}";
        dto.CycleTypeName = GetCycleTypeName(entity.CycleType);

        return ApiResult.Success(dto);
    }

    /// <summary>
    /// 获取点检计划明细
    /// </summary>
    [OpenApiTag("mes/inspection"), OpenApiOperation("获取点检计划明细", "获取点检计划的所有明细项目")]
    [RoutePattern(pattern: "plan/{id}/items", true)]
    public async Task<ApiResult<List<InspectionItemViewDto>>> GetPlanItemsAsync(long planId)
    {
        var items = await ItemRepo.GetByPlanIdAsync(planId);
        var dtos = items.Select(i =>
        {
            var dto = i.Map<InspectionItemViewDto>();
            dto.ItemTypeName = GetItemTypeName(i.ItemType);
            return dto;
        }).ToList();

        return ApiResult.Success(dtos);
    }

    /// <summary>
    /// 创建设检计划
    /// </summary>
    [OpenApiTag("mes/inspection"), OpenApiOperation("创建设检计划", "创建点检计划及其明细")]
    [RoutePattern(pattern: "plan/create", true)]
    public async Task<ApiResult<long>> CreatePlanAsync([FromBody] InspectionPlanCreateCommand command)
    {
        var entity = command.Map<InspectionPlanEntity>();
        entity.IsEnabled = true;
        entity.ItemCount = command.Items?.Count ?? 0;

        await PlanRepo.InsertAsync(entity);

        // 插入明细
        if (command.Items != null && command.Items.Any())
        {
            foreach (var item in command.Items)
            {
                var itemEntity = item.Map<InspectionItemEntity>();
                itemEntity.PlanId = entity.Id;
                await ItemRepo.InsertAsync(itemEntity);
            }
        }

        return ApiResult.Success(entity.Id);
    }

    /// <summary>
    /// 删除点检计划
    /// </summary>
    [OpenApiTag("mes/inspection"), OpenApiOperation("删除点检计划", "删除点检计划及其明细")]
    [RoutePattern(pattern: "plan/delete/{id}", true)]
    public async Task<ApiResult<bool>> DeletePlanAsync(long id)
    {
        await ItemRepo.DeleteByPlanIdAsync(id);
        var entity = await PlanRepo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("点检计划不存在");

        await PlanRepo.DeleteAsync(entity);
        return ApiResult.Success(true);
    }

    /// <summary>
    /// 获取待执行点检计划
    /// </summary>
    [OpenApiTag("mes/inspection"), OpenApiOperation("获取待执行点检计划", "获取已到期的待执行点检计划")]
    [RoutePattern(pattern: "plan/pending", true)]
    public async Task<ApiResult<List<InspectionPlanViewDto>>> GetPendingPlansAsync()
    {
        var list = await PlanRepo.GetPendingPlansAsync(DateTime.Now.AddDays(1));
        var dtos = list.Select(p =>
        {
            var dto = p.Map<InspectionPlanViewDto>();
            dto.Code = $"DJC{p.Id:D8}";
            dto.CycleTypeName = GetCycleTypeName(p.CycleType);
            return dto;
        }).ToList();

        return ApiResult.Success(dtos);
    }

    #endregion

    #region 点检记录

    /// <summary>
    /// 点检记录分页查询
    /// </summary>
    [OpenApiTag("mes/inspection"), OpenApiOperation("点检记录分页查询", "支持按编号、设备、结果、时间筛选")]
    [RoutePattern(pattern: "record/page", true)]
    public async Task<ApiResult<PagedResponse<InspectionRecordViewDto>>> RecordPageAsync([FromBody] InspectionRecordPageQuery query)
    {
        var expr = RecordRepo.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(query.Code), r => r.Code!.Contains(query.Code!))
            .WhereIF(query.EquipmentId.HasValue, r => r.EquipmentId == query.EquipmentId.Value)
            .WhereIF(!string.IsNullOrEmpty(query.EquipmentCode), r => r.EquipmentCode!.Contains(query.EquipmentCode!))
            .WhereIF(!string.IsNullOrEmpty(query.EquipmentName), r => r.EquipmentName!.Contains(query.EquipmentName!))
            .WhereIF(query.Result.HasValue, r => r.Result == query.Result.Value)
            .WhereIF(query.InspectorId.HasValue, r => r.InspectorId == query.InspectorId.Value)
            .WhereIF(query.InspectDateFrom.HasValue, r => r.InspectDate >= query.InspectDateFrom.Value)
            .WhereIF(query.InspectDateTo.HasValue, r => r.InspectDate <= query.InspectDateTo.Value);

        RefAsync<int> total = 0;
        var list = await expr.OrderBy(r => r.InspectDate, OrderByType.Desc)
            .ToPageListAsync(query.Page, query.PageSize, total);

        var dtos = list.Select(r =>
        {
            var dto = r.Map<InspectionRecordViewDto>();
            dto.Code = $"DJR{r.Id:D8}";
            dto.ResultName = GetResultName(r.Result);
            return dto;
        }).ToList();

        return ApiResult.Success(new PagedResponse<InspectionRecordViewDto>(dtos, total));
    }

    /// <summary>
    /// 获取今日点检记录
    /// </summary>
    [OpenApiTag("mes/inspection"), OpenApiOperation("获取今日点检记录", "获取当日的所有点检记录")]
    [RoutePattern(pattern: "record/today", true)]
    public async Task<ApiResult<List<InspectionRecordViewDto>>> GetTodayRecordsAsync()
    {
        var list = await RecordRepo.GetTodayRecordsAsync(null);
        var dtos = list.Select(r =>
        {
            var dto = r.Map<InspectionRecordViewDto>();
            dto.Code = $"DJR{r.Id:D8}";
            dto.ResultName = GetResultName(r.Result);
            return dto;
        }).ToList();

        return ApiResult.Success(dtos);
    }

    /// <summary>
    /// 执行点检
    /// </summary>
    [OpenApiTag("mes/inspection"), OpenApiOperation("执行点检", "执行点检并记录结果")]
    [RoutePattern(pattern: "execute", true)]
    public async Task<ApiResult<bool>> ExecuteAsync([FromBody] InspectionExecuteCommand command)
    {
        var plan = await PlanRepo.GetByIdAsync(command.PlanId)
            ?? throw new UserFriendlyException("点检计划不存在");

        // 创建点检记录
        var record = new InspectionRecordEntity
        {
            Code = $"DJR{DateTime.Now:yyyyMMddHHmmss}",
            PlanId = command.PlanId,
            EquipmentId = plan.EquipmentId,
            EquipmentCode = plan.EquipmentCode,
            EquipmentName = plan.EquipmentName,
            InspectDate = DateTime.Now,
            Result = command.Result,
            CompletedTime = DateTime.Now,
            DurationMinutes = 30, // 默认30分钟
            AbnormalDesc = command.AbnormalDesc,
            CreateRepairOrder = command.CreateRepairOrder,
            Remark = command.Remark
        };

        await RecordRepo.InsertAsync(record);

        // 如果需要生成报修单
        if (command.CreateRepairOrder && command.Result == InspectionResultEnum.Abnormal)
        {
            var repairOrder = new RepairOrderEntity
            {
                Code = $"BX{DateTime.Now:yyyyMMddHHmmss}",
                EquipmentId = plan.EquipmentId,
                EquipmentCode = plan.EquipmentCode,
                EquipmentName = plan.EquipmentName,
                RepairType = 2, // 点检异常
                Description = command.RepairDescription ?? command.AbnormalDesc ?? "点检发现异常",
                Priority = RepairPriorityEnum.High,
                Status = RepairStatusEnum.Pending,
                ReportTime = DateTime.Now
            };

            await RepairRepo.InsertAsync(repairOrder);
            record.RepairOrderId = repairOrder.Id;
            await RecordRepo.UpdateAsync(record);
        }

        // 更新下次点检日期
        var nextDate = command.Result == InspectionResultEnum.Normal
            ? CalculateNextInspectDate(plan.CycleType, plan.CycleValue)
            : DateTime.Now.AddDays(1);
        await PlanRepo.UpdateNextInspectDateAsync(command.PlanId, nextDate);

        return ApiResult.Success(true);
    }

    #endregion

    #region 辅助方法

    private static DateTime CalculateNextInspectDate(int cycleType, int cycleValue)
    {
        return cycleType switch
        {
            1 => DateTime.Now.AddDays(cycleValue), // 每日
            2 => DateTime.Now.AddDays(cycleValue * 7), // 每周
            3 => DateTime.Now.AddMonths(cycleValue), // 每月
            _ => DateTime.Now.AddDays(cycleValue) // 自定义
        };
    }

    private static string GetCycleTypeName(int cycleType) => cycleType switch
    {
        1 => "每日",
        2 => "每周",
        3 => "每月",
        4 => "自定义",
        _ => "未知"
    };

    private static string GetItemTypeName(int itemType) => itemType switch
    {
        1 => "设备状态",
        2 => "运行参数",
        3 => "安全检查",
        4 => "清洁度",
        5 => "润滑",
        6 => "其他",
        _ => "未知"
    };

    private static string GetResultName(InspectionResultEnum result) => result switch
    {
        InspectionResultEnum.Normal => "正常",
        InspectionResultEnum.Abnormal => "异常",
        InspectionResultEnum.NeedRepair => "待维修",
        _ => "未知"
    };

    #endregion
}
