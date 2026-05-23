using Microsoft.AspNetCore.Builder;
using Lzq.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Lzq.WorkOrder.Application.Contracts.WorkOrder.Commands;
using Lzq.WorkOrder.Application.Contracts.WorkOrder.Dto;
using Lzq.WorkOrder.Application.Contracts.WorkOrder.Queries;
using Lzq.WorkOrder.Domain.Entities.WorkOrder;
using Lzq.WorkOrder.Domain.Enums;
using Lzq.WorkOrder.Domain.IRepositories.WorkOrder;
using NSwag.Annotations;
using SqlSugar;
using Mapster;

namespace Lzq.WorkOrder.Application.Services.WorkOrder;

/// <summary>
/// 报工记录服务
/// </summary>
public class WorkReportService : ServiceBase
{
    public WorkReportService() : base("/api/v1/mes/work-report") { }

    private IWorkReportRepository WorkReportRepo => GetRequiredService<IWorkReportRepository>();
    private IWorkOrderRepository WorkOrderRepo => GetRequiredService<IWorkOrderRepository>();

    #region 查询

    /// <summary>
    /// 报工记录分页查询
    /// </summary>
    [OpenApiTag("mes/work-report"), OpenApiOperation("报工记录分页查询", "支持按工单、操作人员、时间范围筛选")]
    [RoutePattern(pattern: "page", true)]
    public async Task<ApiResult<PagedResponse<WorkReportViewDto>>> PageAsync([FromBody] WorkReportPageQuery query)
    {
        var expr = WorkReportRepo.AsQueryable()
            .WhereIF(query.WorkOrderId.HasValue, w => w.WorkOrderId == query.WorkOrderId.Value)
            .WhereIF(!string.IsNullOrEmpty(query.OperatorId), w => w.OperatorId == query.OperatorId)
            .WhereIF(query.ReportTimeFrom.HasValue, w => w.ReportTime >= query.ReportTimeFrom.Value)
            .WhereIF(query.ReportTimeTo.HasValue, w => w.ReportTime <= query.ReportTimeTo.Value);

        RefAsync<int> total = 0;
        var list = await expr.OrderBy(w => w.ReportTime, OrderByType.Desc)
            .ToPageListAsync(query.Page, query.PageSize, total);

        // 填充工单编号
        var workOrderIds = list.Select(w => w.WorkOrderId).Distinct().ToList();
        var workOrders = await WorkOrderRepo.AsQueryable()
            .Where(w => workOrderIds.Contains(w.Id))
            .Select(w => new { w.Id, w.Code })
            .ToListAsync();

        var dtos = list.Select(w =>
        {
            var dto = w.Map<WorkReportViewDto>();
            dto.WorkOrderCode = workOrders.FirstOrDefault(o => o.Id == w.WorkOrderId)?.Code;
            return dto;
        }).ToList();

        return ApiResult.Success(new PagedResponse<WorkReportViewDto>(dtos, total));
    }

    /// <summary>
    /// 获取工单的报工记录列表
    /// </summary>
    [OpenApiTag("mes/work-report"), OpenApiOperation("获取工单的报工记录", "获取指定工单的所有报工记录")]
    [RoutePattern(pattern: "list/{workOrderId}", true)]
    public async Task<ApiResult<List<WorkReportViewDto>>> GetByWorkOrderIdAsync(long workOrderId)
    {
        var list = await WorkReportRepo.GetByWorkOrderIdAsync(workOrderId);
        var dtos = list.Map<List<WorkReportViewDto>>();

        // 填充工单编号
        var workOrder = await WorkOrderRepo.GetByIdAsync(workOrderId);
        if (workOrder != null)
        {
            foreach (var dto in dtos)
            {
                dto.WorkOrderCode = workOrder.Code;
            }
        }

        return ApiResult.Success(dtos);
    }

    #endregion

    #region 报工操作

    /// <summary>
    /// 创建报工记录
    /// </summary>
    [OpenApiTag("mes/c"), OpenApiOperation("创建报工记录", "操作工报工，记录合格数量、不良数量和工时")]
    [RoutePattern(pattern: "create", true)]
    public async Task<ApiResult<long>> CreateAsync([FromBody] WorkReportCreateCommand command)
    {
        // 验证工单存在且状态为生产中
        var workOrder = await WorkOrderRepo.GetByIdAsync(command.WorkOrderId)
            ?? throw new UserFriendlyException("工单不存在");

        if (workOrder.Status != WorkOrderStatusEnum.InProgress)
            throw new UserFriendlyException("只能在生产中的工单上报工");

        // 累计报工数量不能超过计划数量
        var workReports = await WorkReportRepo.AsQueryable().Where(a => a.WorkOrderId.Equals(command.WorkOrderId)).ToListAsync();
        var workReportQty = workReports.Sum(a => a.QualifiedQty + a.DefectQty) + command.DefectQty + command.QualifiedQty;
        if (workReportQty > workOrder.PlannedQty)
            throw new UserFriendlyException("工单累计报工数量:{0},累计报工数量不能超过计划数量".Format(workReportQty));

        var entity = command.Map<WorkReportEntity>();
        entity.ReportTime = DateTime.Now;

        await WorkReportRepo.InsertAsync(entity);

        // 更新工单的已完成数量和不良数量
        var (qualifiedQty, defectQty) = await WorkReportRepo.GetTotalQtyByWorkOrderIdAsync(command.WorkOrderId);
        workOrder.CompletedQty = qualifiedQty;
        workOrder.DefectQty = defectQty;
        await WorkOrderRepo.UpdateAsync(workOrder);

        return ApiResult.Success(entity.Id);
    }

    /// <summary>
    /// 删除报工记录
    /// </summary>
    [OpenApiTag("mes/work-report"), OpenApiOperation("删除报工记录", "删除报工记录，会重新计算工单的完成数量")]
    [RoutePattern(pattern: "delete/{id}", true)]
    public async Task<ApiResult<bool>> DeleteAsync(long id)
    {
        var entity = await WorkReportRepo.GetByIdAsync(id)
            ?? throw new UserFriendlyException("报工记录不存在");

        var workOrderId = entity.WorkOrderId;
        await WorkReportRepo.DeleteAsync(entity);

        // 重新计算工单的完成数量
        var workOrder = await WorkOrderRepo.GetByIdAsync(workOrderId);
        if (workOrder != null)
        {
            var (qualifiedQty, defectQty) = await WorkReportRepo.GetTotalQtyByWorkOrderIdAsync(workOrderId);
            workOrder.CompletedQty = qualifiedQty;
            workOrder.DefectQty = defectQty;
            await WorkOrderRepo.UpdateAsync(workOrder);
        }

        return ApiResult.Success(true);
    }

    #endregion
}
