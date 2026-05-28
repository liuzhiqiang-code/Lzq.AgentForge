using Microsoft.AspNetCore.Builder;
using Lzq.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Lzq.MES.Domain.Enums;
using Mapster;
using NSwag.Annotations;
using SqlSugar;
using Lzq.MES.Application.Contracts.IServices;
using Lzq.MES.Application.Contracts.Commands;
using Lzq.MES.Application.Contracts.Queries;
using Lzq.MES.Application.Contracts.Dtos;
using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.IRepositories;

namespace Lzq.MES.Application.Services;

/// <summary>
/// 保养服务
/// </summary>
public class MaintenanceService : ServiceBase, IMaintenanceService
{
    public MaintenanceService() : base("/api/v1/mes/maintenance") { }

    private IMaintenancePlanRepository PlanRepo => GetRequiredService<IMaintenancePlanRepository>();
    private IMaintenanceRecordRepository RecordRepo => GetRequiredService<IMaintenanceRecordRepository>();

    #region 保养计划

    /// <summary>
    /// 保养计划分页查询
    /// </summary>
    [OpenApiTag("mes/maintenance"), OpenApiOperation("保养计划分页查询", "支持按编号、名称、设备、类型筛选")]
    [RoutePattern(pattern: "plan/page", true)]
    public async Task<ApiResult<PagedResponse<MaintenancePlanViewDto>>> PlanPageAsync([FromBody] MaintenancePlanPageQuery query)
    {
        var expr = PlanRepo.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(query.Code), p => p.Code!.Contains(query.Code!))
            .WhereIF(!string.IsNullOrEmpty(query.Name), p => p.Name!.Contains(query.Name!))
            .WhereIF(query.EquipmentId.HasValue, p => p.EquipmentId == query.EquipmentId.Value)
            .WhereIF(query.MaintenanceType.HasValue, p => p.MaintenanceType == query.MaintenanceType.Value)
            .WhereIF(query.Status.HasValue, p => p.Status == query.Status.Value)
            .WhereIF(query.PlanDateFrom.HasValue, p => p.PlanDate >= query.PlanDateFrom.Value)
            .WhereIF(query.PlanDateTo.HasValue, p => p.PlanDate <= query.PlanDateTo.Value);

        RefAsync<int> total = 0;
        var list = await expr.OrderBy(p => p.PlanDate)
            .ToPageListAsync(query.Page, query.PageSize, total);

        var dtos = list.Select(p =>
        {
            var dto = p.Map<MaintenancePlanViewDto>();
            dto.MaintenanceTypeName = GetMaintenanceTypeName(p.MaintenanceType);
            dto.StatusName = GetPlanStatusName(p.Status);
            return dto;
        }).ToList();

        return ApiResult.Success(new PagedResponse<MaintenancePlanViewDto>(dtos, total));
    }

    /// <summary>
    /// 获取保养计划详情
    /// </summary>
    [OpenApiTag("mes/maintenance"), OpenApiOperation("获取保养计划详情", "根据ID获取保养计划详细信息")]
    [RoutePattern(pattern: "plan/{id}", true)]
    public async Task<ApiResult<MaintenancePlanViewDto>> GetPlanAsync(long id)
    {
        var entity = await PlanRepo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("保养计划不存在");

        var dto = entity.Map<MaintenancePlanViewDto>();
        dto.MaintenanceTypeName = GetMaintenanceTypeName(entity.MaintenanceType);
        dto.StatusName = GetPlanStatusName(entity.Status);

        return ApiResult.Success(dto);
    }

    /// <summary>
    /// 创保养计划
    /// </summary>
    [OpenApiTag("mes/maintenance"), OpenApiOperation("创保养计划", "创建设备保养计划")]
    [RoutePattern(pattern: "plan/create", true)]
    public async Task<ApiResult<long>> CreatePlanAsync([FromBody] MaintenancePlanCreateCommand command)
    {
        var entity = command.Map<MaintenancePlanEntity>();
        entity.Code = $"BY{DateTime.Now:yyyyMMddHHmmss}";
        entity.Status = MaintenancePlanStatusEnum.Pending;

        await PlanRepo.InsertAsync(entity);

        return ApiResult.Success(entity.Id);
    }

    /// <summary>
    /// 删除保养计划
    /// </summary>
    [OpenApiTag("mes/maintenance"), OpenApiOperation("删除保养计划", "删除保养计划")]
    [RoutePattern(pattern: "plan/delete/{id}", true)]
    public async Task<ApiResult<bool>> DeletePlanAsync(long id)
    {
        var entity = await PlanRepo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("保养计划不存在");

        await PlanRepo.DeleteAsync(entity);

        return ApiResult.Success(true);
    }

    #endregion

    #region 保养记录

    /// <summary>
    /// 保养记录分页查询
    /// </summary>
    [OpenApiTag("mes/maintenance"), OpenApiOperation("保养记录分页查询", "支持按编号、设备、类型、时间筛选")]
    [RoutePattern(pattern: "record/page", true)]
    public async Task<ApiResult<PagedResponse<MaintenanceRecordViewDto>>> RecordPageAsync([FromBody] MaintenanceRecordPageQuery query)
    {
        var expr = RecordRepo.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(query.Code), r => r.Code!.Contains(query.Code!))
            .WhereIF(query.EquipmentId.HasValue, r => r.EquipmentId == query.EquipmentId.Value)
            .WhereIF(!string.IsNullOrEmpty(query.EquipmentName), r => r.EquipmentName!.Contains(query.EquipmentName!))
            .WhereIF(query.MaintenanceType.HasValue, r => r.MaintenanceType == query.MaintenanceType.Value)
            .WhereIF(query.MaintenanceDateFrom.HasValue, r => r.MaintenanceDate >= query.MaintenanceDateFrom.Value)
            .WhereIF(query.MaintenanceDateTo.HasValue, r => r.MaintenanceDate <= query.MaintenanceDateTo.Value);

        RefAsync<int> total = 0;
        var list = await expr.OrderBy(r => r.MaintenanceDate, OrderByType.Desc)
            .ToPageListAsync(query.Page, query.PageSize, total);

        var dtos = list.Select(r =>
        {
            var dto = r.Map<MaintenanceRecordViewDto>();
            dto.MaintenanceTypeName = GetMaintenanceTypeName(r.MaintenanceType);
            return dto;
        }).ToList();

        return ApiResult.Success(new PagedResponse<MaintenanceRecordViewDto>(dtos, total));
    }

    /// <summary>
    /// 创建保养记录
    /// </summary>
    [OpenApiTag("mes/maintenance"), OpenApiOperation("创建保养记录", "完成保养后创建保养记录")]
    [RoutePattern(pattern: "record/create", true)]
    public async Task<ApiResult<long>> CreateRecordAsync([FromBody] MaintenanceRecordCreateCommand command)
    {
        var entity = command.Map<MaintenanceRecordEntity>();
        entity.Code = $"BYJL{DateTime.Now:yyyyMMddHHmmss}";

        await RecordRepo.InsertAsync(entity);

        // 更新保养计划状态
        if (command.PlanId.HasValue)
        {
            var plan = await PlanRepo.GetByIdAsync(command.PlanId.Value);
            if (plan != null)
            {
                plan.Status = MaintenancePlanStatusEnum.Completed;
                plan.ActualDate = command.MaintenanceDate;

                // 计算下次保养日期
                plan.PlanDate = command.MaintenanceDate.AddDays(plan.CycleDays);
                plan.Status = MaintenancePlanStatusEnum.Pending;

                await PlanRepo.UpdateAsync(plan);
            }
        }

        return ApiResult.Success(entity.Id);
    }

    #endregion

    #region 辅助方法

    private static string GetMaintenanceTypeName(MaintenanceTypeEnum type) => type switch
    {
        MaintenanceTypeEnum.Daily => "日常保养",
        MaintenanceTypeEnum.Level1 => "一级保养",
        MaintenanceTypeEnum.Level2 => "二级保养",
        MaintenanceTypeEnum.Level3 => "三级保养",
        MaintenanceTypeEnum.Precision => "精度保养",
        _ => "未知"
    };

    private static string GetPlanStatusName(MaintenancePlanStatusEnum status) => status switch
    {
        MaintenancePlanStatusEnum.Pending => "待执行",
        MaintenancePlanStatusEnum.InProgress => "执行中",
        MaintenancePlanStatusEnum.Completed => "已完成",
        MaintenancePlanStatusEnum.Delayed => "已延期",
        MaintenancePlanStatusEnum.Cancelled => "已取消",
        _ => "未知"
    };

    #endregion
}
