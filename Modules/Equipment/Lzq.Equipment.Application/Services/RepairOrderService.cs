using Microsoft.AspNetCore.Builder;
using Lzq.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Lzq.Equipment.Domain.Enums;
using Mapster;
using NSwag.Annotations;
using SqlSugar;
using Lzq.Equipment.Application.Contracts.IServices;
using Lzq.Equipment.Application.Contracts.Commands;
using Lzq.Equipment.Application.Contracts.Queries;
using Lzq.Equipment.Application.Contracts.Dtos;
using Lzq.Equipment.Domain.Entities;
using Lzq.Equipment.Domain.IRepositories;

namespace Lzq.Equipment.Application.Services;

/// <summary>
/// 维修服务
/// </summary>
public class RepairOrderService : ServiceBase, IRepairOrderService
{
    public RepairOrderService() : base("/api/v1/mes/repair") { }

    private IRepairOrderRepository RepairRepo => GetRequiredService<IRepairOrderRepository>();
    private IEquipmentRepository EquipmentRepo => GetRequiredService<IEquipmentRepository>();

    /// <summary>
    /// 报修单分页查询
    /// </summary>
    [OpenApiTag("mes/repair"), OpenApiOperation("报修单分页查询", "支持按编号、设备、状态、优先级筛选")]
    [RoutePattern(pattern: "page", true)]
    public async Task<ApiResult<PagedResponse<RepairOrderViewDto>>> PageAsync([FromBody] RepairOrderPageQuery query)
    {
        var expr = RepairRepo.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(query.Code), r => r.Code!.Contains(query.Code!))
            .WhereIF(query.EquipmentId.HasValue, r => r.EquipmentId == query.EquipmentId.Value)
            .WhereIF(!string.IsNullOrEmpty(query.EquipmentCode), r => r.EquipmentCode!.Contains(query.EquipmentCode!))
            .WhereIF(!string.IsNullOrEmpty(query.EquipmentName), r => r.EquipmentName!.Contains(query.EquipmentName!))
            .WhereIF(query.RepairType.HasValue, r => r.RepairType == query.RepairType.Value)
            .WhereIF(query.Status.HasValue, r => r.Status == query.Status.Value)
            .WhereIF(query.Priority.HasValue, r => r.Priority == query.Priority.Value)
            .WhereIF(query.ReporterId.HasValue, r => r.ReporterId == query.ReporterId.Value)
            .WhereIF(query.RepairUserId.HasValue, r => r.RepairUserId == query.RepairUserId.Value)
            .WhereIF(query.ReportTimeFrom.HasValue, r => r.ReportTime >= query.ReportTimeFrom.Value)
            .WhereIF(query.ReportTimeTo.HasValue, r => r.ReportTime <= query.ReportTimeTo.Value);

        RefAsync<int> total = 0;
        var list = await expr.OrderBy(r => r.Priority, OrderByType.Desc)
            .OrderBy(r => r.ReportTime, OrderByType.Desc)
            .ToPageListAsync(query.Page, query.PageSize, total);

        var dtos = list.Select(r =>
        {
            var dto = r.Map<RepairOrderViewDto>();
            dto.RepairTypeName = GetRepairTypeName(r.RepairType);
            dto.PriorityName = GetPriorityName(r.Priority);
            dto.StatusName = GetStatusName(r.Status);
            return dto;
        }).ToList();

        return ApiResult.Success(new PagedResponse<RepairOrderViewDto>(dtos, total));
    }

    /// <summary>
    /// 获取报修单详情
    /// </summary>
    [OpenApiTag("mes/repair"), OpenApiOperation("获取报修单详情", "根据ID获取报修单详细信息")]
    [RoutePattern(pattern: "{id}", true)]
    public async Task<ApiResult<RepairOrderViewDto>> GetAsync(long id)
    {
        var entity = await RepairRepo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("报修单不存在");

        var dto = entity.Map<RepairOrderViewDto>();
        dto.RepairTypeName = GetRepairTypeName(entity.RepairType);
        dto.PriorityName = GetPriorityName(entity.Priority);
        dto.StatusName = GetStatusName(entity.Status);

        return ApiResult.Success(dto);
    }

    /// <summary>
    /// 创建报修单
    /// </summary>
    [OpenApiTag("mes/repair"), OpenApiOperation("创建报修单", "创建设备报修单")]
    [RoutePattern(pattern: "create", true)]
    public async Task<ApiResult<long>> CreateAsync([FromBody] RepairOrderCreateCommand command)
    {
        var entity = command.Map<RepairOrderEntity>();
        entity.Code = $"BX{DateTime.Now:yyyyMMddHHmmss}";
        entity.Status = RepairStatusEnum.Pending;
        entity.ReportTime = DateTime.Now;

        await RepairRepo.InsertAsync(entity);

        // 更新设备状态为维修中
        var equipment = await EquipmentRepo.GetByIdAsync(command.EquipmentId);
        if (equipment != null)
        {
            equipment.Status = EquipmentStatusEnum.UnderRepair;
            await EquipmentRepo.UpdateAsync(equipment);
        }

        return ApiResult.Success(entity.Id);
    }

    /// <summary>
    /// 派工
    /// </summary>
    [OpenApiTag("mes/repair"), OpenApiOperation("派工", "指派维修人员")]
    [RoutePattern(pattern: "assign", true)]
    public async Task<ApiResult<bool>> AssignAsync([FromBody] RepairAssignCommand command)
    {
        var entity = await RepairRepo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("报修单不存在");

        if (entity.Status != RepairStatusEnum.Pending)
            throw new UserFriendlyException("只能对待派工的报修单进行派工");

        entity.Status = RepairStatusEnum.Assigned;
        entity.RepairUserId = command.RepairUserId;
        entity.RepairUserName = command.RepairUserName;

        await RepairRepo.UpdateAsync(entity);

        return ApiResult.Success(true);
    }

    /// <summary>
    /// 维修开始
    /// </summary>
    [OpenApiTag("mes/repair"), OpenApiOperation("维修开始", "开始维修作业")]
    [RoutePattern(pattern: "start", true)]
    public async Task<ApiResult<bool>> StartAsync([FromBody] RepairStartCommand command)
    {
        var entity = await RepairRepo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("报修单不存在");

        if (entity.Status != RepairStatusEnum.Assigned)
            throw new UserFriendlyException("只能对已派工的报修单开始维修");

        entity.Status = RepairStatusEnum.InProgress;
        entity.RepairStartTime = DateTime.Now;

        await RepairRepo.UpdateAsync(entity);

        return ApiResult.Success(true);
    }

    /// <summary>
    /// 维修完成
    /// </summary>
    [OpenApiTag("mes/repair"), OpenApiOperation("维修完成", "完成维修作业")]
    [RoutePattern(pattern: "complete", true)]
    public async Task<ApiResult<bool>> CompleteAsync([FromBody] RepairCompleteCommand command)
    {
        var entity = await RepairRepo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("报修单不存在");

        if (entity.Status != RepairStatusEnum.InProgress)
            throw new UserFriendlyException("只能对维修中的报修单完成维修");

        entity.Status = RepairStatusEnum.Completed;
        entity.RepairEndTime = DateTime.Now;
        entity.FaultReason = command.FaultReason;
        entity.RepairProcess = command.RepairProcess;
        entity.PartsUsed = command.PartsUsed;
        entity.WorkHours = command.WorkHours;
        entity.Cost = command.Cost;
        if (!string.IsNullOrEmpty(command.Remark))
        {
            entity.Remark = string.IsNullOrEmpty(entity.Remark)
                ? command.Remark
                : $"{entity.Remark}\n{command.Remark}";
        }

        await RepairRepo.UpdateAsync(entity);

        return ApiResult.Success(true);
    }

    /// <summary>
    /// 验收
    /// </summary>
    [OpenApiTag("mes/repair"), OpenApiOperation("验收", "验收维修结果")]
    [RoutePattern(pattern: "accept", true)]
    public async Task<ApiResult<bool>> AcceptAsync([FromBody] RepairAcceptCommand command)
    {
        var entity = await RepairRepo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("报修单不存在");

        if (entity.Status != RepairStatusEnum.Completed)
            throw new UserFriendlyException("只能对已完工的报修单进行验收");

        entity.Status = RepairStatusEnum.Accepted;
        entity.AcceptTime = DateTime.Now;
        entity.AcceptComment = command.AcceptComment;
        if (!string.IsNullOrEmpty(command.Remark))
        {
            entity.Remark = string.IsNullOrEmpty(entity.Remark)
                ? command.Remark
                : $"{entity.Remark}\n{command.Remark}";
        }

        await RepairRepo.UpdateAsync(entity);

        // 更新设备状态为正常
        var equipment = await EquipmentRepo.GetByIdAsync(entity.EquipmentId);
        if (equipment != null)
        {
            equipment.Status = EquipmentStatusEnum.Normal;
            equipment.TotalRepairCount++;
            await EquipmentRepo.UpdateAsync(equipment);
        }

        return ApiResult.Success(true);
    }

    /// <summary>
    /// 取消报修单
    /// </summary>
    [OpenApiTag("mes/repair"), OpenApiOperation("取消报修单", "取消报修单")]
    [RoutePattern(pattern: "cancel/{id}", true)]
    public async Task<ApiResult<bool>> CancelAsync(long id)
    {
        var entity = await RepairRepo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("报修单不存在");

        if (entity.Status == RepairStatusEnum.Accepted)
            throw new UserFriendlyException("已验收的报修单不能取消");

        entity.Status = RepairStatusEnum.Cancelled;

        await RepairRepo.UpdateAsync(entity);

        // 如果设备状态是维修中，恢复为正常
        var equipment = await EquipmentRepo.GetByIdAsync(entity.EquipmentId);
        if (equipment != null && equipment.Status == EquipmentStatusEnum.UnderRepair)
        {
            equipment.Status = EquipmentStatusEnum.Normal;
            await EquipmentRepo.UpdateAsync(equipment);
        }

        return ApiResult.Success(true);
    }

    /// <summary>
    /// 获取维修统计
    /// </summary>
    [OpenApiTag("mes/repair"), OpenApiOperation("维修统计", "获取维修单统计信息")]
    [RoutePattern(pattern: "statistics", true)]
    public async Task<ApiResult<RepairStatisticsDto>> GetStatisticsAsync(DateTime? fromDate, DateTime? toDate)
    {
        var (pendingCount, inProgressCount, completedCount) = await RepairRepo.GetStatisticsAsync(fromDate, toDate);

        var totalCount = await RepairRepo.AsQueryable()
            .WhereIF(fromDate.HasValue, r => r.ReportTime >= fromDate.Value)
            .WhereIF(toDate.HasValue, r => r.ReportTime <= toDate.Value)
            .CountAsync();

        var totalWorkHours = await RepairRepo.AsQueryable()
            .Where(r => r.Status == RepairStatusEnum.Accepted)
            .WhereIF(fromDate.HasValue, r => r.ReportTime >= fromDate.Value)
            .WhereIF(toDate.HasValue, r => r.ReportTime <= toDate.Value)
            .SumAsync(r => r.WorkHours);

        var totalCost = await RepairRepo.AsQueryable()
            .Where(r => r.Status == RepairStatusEnum.Accepted)
            .WhereIF(fromDate.HasValue, r => r.ReportTime >= fromDate.Value)
            .WhereIF(toDate.HasValue, r => r.ReportTime <= toDate.Value)
            .SumAsync(r => r.Cost);

        var urgentCount = await RepairRepo.AsQueryable()
            .Where(r => r.Priority == RepairPriorityEnum.Urgent)
            .WhereIF(fromDate.HasValue, r => r.ReportTime >= fromDate.Value)
            .WhereIF(toDate.HasValue, r => r.ReportTime <= toDate.Value)
            .CountAsync();

        var dto = new RepairStatisticsDto
        {
            TotalCount = totalCount,
            PendingCount = pendingCount,
            InProgressCount = inProgressCount,
            CompletedCount = completedCount,
            TotalWorkHours = totalWorkHours,
            TotalCost = totalCost,
            UrgentCount = urgentCount
        };

        return ApiResult.Success(dto);
    }

    #region 辅助方法

    private static string GetRepairTypeName(int type) => type switch
    {
        1 => "故障报修",
        2 => "点检异常",
        3 => "计划维修",
        4 => "其他",
        _ => "未知"
    };

    private static string GetPriorityName(RepairPriorityEnum priority) => priority switch
    {
        RepairPriorityEnum.Urgent => "紧急",
        RepairPriorityEnum.High => "高",
        RepairPriorityEnum.Medium => "中",
        RepairPriorityEnum.Low => "低",
        _ => "未知"
    };

    private static string GetStatusName(RepairStatusEnum status) => status switch
    {
        RepairStatusEnum.Pending => "待派工",
        RepairStatusEnum.Assigned => "已派工",
        RepairStatusEnum.InProgress => "维修中",
        RepairStatusEnum.Completed => "已完工",
        RepairStatusEnum.Accepted => "已验收",
        RepairStatusEnum.Cancelled => "已取消",
        _ => "未知"
    };

    #endregion
}
