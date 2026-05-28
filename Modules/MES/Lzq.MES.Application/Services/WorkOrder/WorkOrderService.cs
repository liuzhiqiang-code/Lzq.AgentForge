using Microsoft.AspNetCore.Builder;
using Lzq.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Lzq.MES.Application.Contracts.WorkOrder.Commands;
using Lzq.MES.Application.Contracts.WorkOrder.Dto;
using Lzq.MES.Application.Contracts.WorkOrder.Queries;
using Lzq.MES.Application.Contracts.IServices;
using Lzq.MES.Application.Contracts.IServices.WorkOrder;
using Lzq.MES.Domain.Entities.WorkOrder;
using Lzq.MES.Domain.Enums;
using Lzq.MES.Domain.IRepositories.WorkOrder;
using NSwag.Annotations;
using SqlSugar;
using Mapster;

namespace Lzq.MES.Application.Services.WorkOrder;

/// <summary>
/// 工单管理服务
/// </summary>
public class WorkOrderService : ServiceBase, IWorkOrderService
{
    public WorkOrderService() : base("/api/v1/mes/work-order") { }

    private IWorkOrderRepository WorkOrderRepo => GetRequiredService<IWorkOrderRepository>();
    private IWorkReportRepository WorkReportRepo => GetRequiredService<IWorkReportRepository>();
    private IReferenceDataService ReferenceDataService => GetRequiredService<IReferenceDataService>();

    #region 查询

    /// <summary>
    /// 工单分页查询
    /// </summary>
    [OpenApiTag("mes/work-order"), OpenApiOperation("工单分页查询", "支持按编号、产品名称、产线、工序、状态、时间范围筛选")]
    [RoutePattern(pattern: "page", true)]
    public async Task<ApiResult<PagedResponse<WorkOrderViewDto>>> PageAsync([FromBody] WorkOrderPageQuery query)
    {
        // 构建查询表达式（产线和工序ID筛选直接使用传入的值）
        var expr = WorkOrderRepo.AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(query.Code), w => w.Code!.Contains(query.Code!))
            .WhereIF(!string.IsNullOrEmpty(query.ProductName), w => w.ProductName!.Contains(query.ProductName!))
            .WhereIF(query.LineId.HasValue, w => w.LineId == query.LineId.Value)
            .WhereIF(query.ProcessId.HasValue, w => w.ProcessId == query.ProcessId.Value)
            .WhereIF(query.Status.HasValue, w => w.Status == query.Status.Value)
            .WhereIF(query.PlannedStartFrom.HasValue, w => w.PlannedStart >= query.PlannedStartFrom.Value)
            .WhereIF(query.PlannedStartTo.HasValue, w => w.PlannedStart <= query.PlannedStartTo.Value)
            .WhereIF(query.CreateTimeFrom.HasValue, w => w.CreationTime >= query.CreateTimeFrom.Value)
            .WhereIF(query.CreateTimeTo.HasValue, w => w.CreationTime <= query.CreateTimeTo.Value);

        RefAsync<int> total = 0;
        var list = await expr.OrderByDescending(w => w.Priority)
            .OrderByDescending(w => w.CreationTime)
            .ToPageListAsync(query.Page, query.PageSize, total);

        // 通过 ReferenceDataService 批量获取产线和工序名称（解耦）
        var lineIdList = list.Select(w => w.LineId).Distinct().ToList();
        var processIdList = list.Select(w => w.ProcessId).Distinct().ToList();
        var lines = await ReferenceDataService.GetLinesByIdsAsync(lineIdList);
        var processes = await ReferenceDataService.GetProcessesByIdsAsync(processIdList);

        var dtos = list.Select(w =>
        {
            var dto = w.Map<WorkOrderViewDto>();
            dto.StatusName = GetStatusName(w.Status);
            dto.LineName = lines.FirstOrDefault(l => l.Id == w.LineId)?.Name;
            dto.ProcessName = processes.FirstOrDefault(p => p.Id == w.ProcessId)?.Name;
            return dto;
        }).ToList();

        return ApiResult<PagedResponse<WorkOrderViewDto>>.Success(new PagedResponse<WorkOrderViewDto>(dtos, total));
    }

    /// <summary>
    /// 获取工单详情
    /// </summary>
    [OpenApiTag("mes/work-order"), OpenApiOperation("获取工单详情", "根据ID获取工单详细信息，包括完成进度")]
    [RoutePattern(pattern: "{id}", true)]
    public async Task<ApiResult<WorkOrderViewDto>> GetAsync(long id)
    {
        var entity = await WorkOrderRepo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("工单不存在");

        // 通过 ReferenceDataService 获取产线和工序名称（解耦）
        var line = await ReferenceDataService.GetLineByIdAsync(entity.LineId);
        var process = await ReferenceDataService.GetProcessByIdAsync(entity.ProcessId);

        var dto = entity.Map<WorkOrderViewDto>();
        dto.StatusName = GetStatusName(entity.Status);
        dto.LineName = line?.Name;
        dto.ProcessName = process?.Name;

        return ApiResult<WorkOrderViewDto>.Success(dto);
    }

    #endregion

    #region CRUD

    /// <summary>
    /// 创建工单
    /// </summary>
    [OpenApiTag("mes/work-order"), OpenApiOperation("创建工单", "新建生产工单，编号不能重复")]
    [RoutePattern(pattern: "create", true)]
    public async Task<ApiResult<long>> CreateAsync([FromBody] WorkOrderCreateCommand command)
    {
        var exists = await WorkOrderRepo.ExistsByCodeAsync(command.Code);
        if (exists) throw new UserFriendlyException($"工单编号 [{command.Code}] 已存在");

        // 通过 ReferenceDataService 验证产线和工序是否存在（解耦）
        if (!await ReferenceDataService.LineExistsAsync(command.LineId))
            throw new UserFriendlyException("指定的产线不存在");
        if (!await ReferenceDataService.ProcessExistsAsync(command.ProcessId))
            throw new UserFriendlyException("指定的工序不存在");

        var entity = command.Map<WorkOrderEntity>();
        entity.Status = WorkOrderStatusEnum.Draft;
        entity.CompletedQty = 0;
        entity.DefectQty = 0;

        await WorkOrderRepo.InsertAsync(entity);
        return ApiResult<long>.Success(entity.Id);
    }

    /// <summary>
    /// 更新工单
    /// </summary>
    [OpenApiTag("mes/work-order"), OpenApiOperation("更新工单", "根据ID更新工单信息，只能更新草稿状态的工单")]
    [RoutePattern(pattern: "update", true)]
    public async Task<ApiResult<bool>> UpdateAsync([FromBody] WorkOrderUpdateCommand command)
    {
        var entity = await WorkOrderRepo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("工单不存在");

        if (entity.Status != WorkOrderStatusEnum.Draft)
            throw new UserFriendlyException("只能更新草稿状态的工单");

        // 通过 ReferenceDataService 验证产线和工序（解耦）
        if (command.LineId != entity.LineId && !await ReferenceDataService.LineExistsAsync(command.LineId))
            throw new UserFriendlyException("指定的产线不存在");
        if (command.ProcessId != entity.ProcessId && !await ReferenceDataService.ProcessExistsAsync(command.ProcessId))
            throw new UserFriendlyException("指定的工序不存在");

        command.Map(entity);
        await WorkOrderRepo.UpdateAsync(entity);
        return ApiResult<bool>.Success(true);
    }

    /// <summary>
    /// 删除工单
    /// </summary>
    [OpenApiTag("mes/work-order"), OpenApiOperation("删除工单", "删除工单，只能删除草稿状态的工单")]
    [RoutePattern(pattern: "delete/{id}", true)]
    public async Task<ApiResult<bool>> DeleteAsync(long id)
    {
        var entity = await WorkOrderRepo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("工单不存在");

        if (entity.Status != WorkOrderStatusEnum.Draft)
            throw new UserFriendlyException("只能删除草稿状态的工单");

        await WorkOrderRepo.DeleteAsync(entity);
        return ApiResult<bool>.Success(true);
    }

    /// <summary>
    /// 批量删除工单
    /// </summary>
    [OpenApiTag("mes/work-order"), OpenApiOperation("批量删除工单", "批量删除工单，只删除草稿状态的工单")]
    [RoutePattern(pattern: "batch-delete", true)]
    public async Task<ApiResult<int>> BatchDeleteAsync([FromBody] List<long> ids)
    {
        if (ids == null || ids.Count == 0)
            throw new UserFriendlyException("请选择要删除的工单");

        var count = await WorkOrderRepo.AsQueryable()
            .Where(w => ids.Contains(w.Id) && w.Status == WorkOrderStatusEnum.Draft)
            .CountAsync();

        await WorkOrderRepo.DeleteAsync(w => ids.Contains(w.Id) && w.Status == WorkOrderStatusEnum.Draft);
        return ApiResult<int>.Success(count);
    }

    #endregion

    #region 状态流转

    /// <summary>
    /// 派发工单（草稿 → 已派发）
    /// </summary>
    [OpenApiTag("mes/work-order"), OpenApiOperation("派发工单", "将工单从草稿状态派发到指定产线")]
    [RoutePattern(pattern: "dispatch", true)]
    public async Task<ApiResult<bool>> DispatchAsync([FromBody] WorkOrderDispatchCommand command)
    {
        var entity = await WorkOrderRepo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("工单不存在");

        if (entity.Status != WorkOrderStatusEnum.Draft)
            throw new UserFriendlyException("只能派发草稿状态的工单");

        entity.Status = WorkOrderStatusEnum.Dispatched;
        await WorkOrderRepo.UpdateAsync(entity);

        // 发布派发事件（可扩展为 RabbitMQ 事件）
        // await PublishEventAsync(new WorkOrderDispatchedEvent(entity.Id, entity.Code));

        return ApiResult<bool>.Success(true);
    }

    /// <summary>
    /// 开工（已派发 → 生产中）
    /// </summary>
    [OpenApiTag("mes/work-order"), OpenApiOperation("开工", "工单开始生产")]
    [RoutePattern(pattern: "start", true)]
    public async Task<ApiResult<bool>> StartAsync([FromBody] WorkOrderStartCommand command)
    {
        var entity = await WorkOrderRepo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("工单不存在");

        if (entity.Status != WorkOrderStatusEnum.Dispatched)
            throw new UserFriendlyException("只能开工已派发的工单");

        entity.Status = WorkOrderStatusEnum.InProgress;
        entity.ActualStart = DateTime.Now;
        await WorkOrderRepo.UpdateAsync(entity);

        return ApiResult<bool>.Success(true);
    }

    /// <summary>
    /// 完工（生产中 → 已完成）
    /// </summary>
    [OpenApiTag("mes/work-order"), OpenApiOperation("完工", "工单完成生产")]
    [RoutePattern(pattern: "complete", true)]
    public async Task<ApiResult<bool>> CompleteAsync([FromBody] WorkOrderCompleteCommand command)
    {
        var entity = await WorkOrderRepo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("工单不存在");

        if (entity.Status != WorkOrderStatusEnum.InProgress)
            throw new UserFriendlyException("只能完工生产中的工单");

        // 同步报工数据，更新工单完成数量
        var (qualifiedQty, defectQty) = await WorkReportRepo.GetTotalQtyByWorkOrderIdAsync(command.Id);
        entity.CompletedQty = qualifiedQty;
        entity.DefectQty = defectQty;
        entity.Status = WorkOrderStatusEnum.Completed;
        entity.ActualEnd = DateTime.Now;
        await WorkOrderRepo.UpdateAsync(entity);

        return ApiResult<bool>.Success(true);
    }

    /// <summary>
    /// 关闭工单（已完成 → 已关闭）
    /// </summary>
    [OpenApiTag("mes/work-order"), OpenApiOperation("关闭工单", "关闭已完成的工单")]
    [RoutePattern(pattern: "close", true)]
    public async Task<ApiResult<bool>> CloseAsync([FromBody] WorkOrderCloseCommand command)
    {
        var entity = await WorkOrderRepo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("工单不存在");

        if (entity.Status != WorkOrderStatusEnum.Completed)
            throw new UserFriendlyException("只能关闭已完成的工单");

        entity.Status = WorkOrderStatusEnum.Closed;
        await WorkOrderRepo.UpdateAsync(entity);

        return ApiResult<bool>.Success(true);
    }

    /// <summary>
    /// 暂停工单（生产中 → 已暂停）
    /// </summary>
    [OpenApiTag("mes/work-order"), OpenApiOperation("暂停工单", "暂停生产中的工单")]
    [RoutePattern(pattern: "pause", true)]
    public async Task<ApiResult<bool>> PauseAsync([FromBody] WorkOrderPauseCommand command)
    {
        var entity = await WorkOrderRepo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("工单不存在");

        if (entity.Status != WorkOrderStatusEnum.InProgress)
            throw new UserFriendlyException("只能暂停生产中的工单");

        entity.Status = WorkOrderStatusEnum.Paused;
        if (!string.IsNullOrEmpty(command.Reason))
        {
            entity.Remark = string.IsNullOrEmpty(entity.Remark)
                ? $"暂停原因：{command.Reason}"
                : $"{entity.Remark}\n暂停原因：{command.Reason}";
        }
        await WorkOrderRepo.UpdateAsync(entity);

        return ApiResult<bool>.Success(true);
    }

    /// <summary>
    /// 取消工单（已派发 → 已取消）
    /// </summary>
    [OpenApiTag("mes/work-order"), OpenApiOperation("取消工单", "取消已派发的工单")]
    [RoutePattern(pattern: "cancel", true)]
    public async Task<ApiResult<bool>> CancelAsync([FromBody] WorkOrderCancelCommand command)
    {
        var entity = await WorkOrderRepo.GetByIdAsync(command.Id)
            ?? throw new UserFriendlyException("工单不存在");

        if (entity.Status != WorkOrderStatusEnum.Dispatched)
            throw new UserFriendlyException("只能取消已派发的工单");

        entity.Status = WorkOrderStatusEnum.Cancelled;
        if (!string.IsNullOrEmpty(command.Reason))
        {
            entity.Remark = string.IsNullOrEmpty(entity.Remark)
                ? $"取消原因：{command.Reason}"
                : $"{entity.Remark}\n取消原因：{command.Reason}";
        }
        await WorkOrderRepo.UpdateAsync(entity);

        return ApiResult<bool>.Success(true);
    }

    #endregion

    #region 辅助方法

    private static string GetStatusName(WorkOrderStatusEnum status) => status switch
    {
        WorkOrderStatusEnum.Draft => "草稿",
        WorkOrderStatusEnum.Dispatched => "已派发",
        WorkOrderStatusEnum.InProgress => "生产中",
        WorkOrderStatusEnum.Completed => "已完成",
        WorkOrderStatusEnum.Closed => "已关闭",
        WorkOrderStatusEnum.Paused => "已暂停",
        WorkOrderStatusEnum.Cancelled => "已取消",
        _ => "未知"
    };

    #endregion
}
