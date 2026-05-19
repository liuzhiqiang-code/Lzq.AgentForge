using FluentAssertions;
using Lzq.Core.Models;
using Lzq.WorkOrder.Application.Contracts.WorkOrder.Commands;
using Lzq.WorkOrder.Application.Contracts.WorkOrder.Dto;
using Lzq.WorkOrder.Application.Contracts.WorkOrder.Queries;
using Lzq.WorkOrder.Domain.Entities.WorkOrder;
using Lzq.WorkOrder.Domain.Enums;
using Lzq.WorkOrder.Tests.Helpers;
using Moq;
using Xunit;

namespace Lzq.WorkOrder.Tests.Services;

public class WorkOrderServiceTests : ServiceTestBase
{
    // ============================================================
    // PageAsync
    // ============================================================

    [Fact]
    public async Task PageAsync_ShouldReturnPagedWorkOrders()
    {
        Db.Seed(
            new WorkOrderEntity { Code = "WO001", ProductName = "产品A", LineId = 1, ProcessId = 1, PlannedQty = 100, Status = WorkOrderStatusEnum.Draft },
            new WorkOrderEntity { Code = "WO002", ProductName = "产品B", LineId = 2, ProcessId = 2, PlannedQty = 200, Status = WorkOrderStatusEnum.InProgress }
        );

        var query = new WorkOrderPageQuery { Page = 1, PageSize = 10 };

        var result = await WorkOrderService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Total.Should().Be(2);
    }

    [Fact]
    public async Task PageAsync_ShouldFilterByStatus()
    {
        Db.Seed(
            new WorkOrderEntity { Code = "WO001", ProductName = "产品A", LineId = 1, ProcessId = 1, PlannedQty = 100, Status = WorkOrderStatusEnum.Draft },
            new WorkOrderEntity { Code = "WO002", ProductName = "产品B", LineId = 2, ProcessId = 2, PlannedQty = 200, Status = WorkOrderStatusEnum.Completed }
        );

        var query = new WorkOrderPageQuery { Status = WorkOrderStatusEnum.Draft, Page = 1, PageSize = 10 };

        var result = await WorkOrderService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Total.Should().Be(1);
    }

    [Fact]
    public async Task PageAsync_ShouldPaginate()
    {
        for (int i = 1; i <= 15; i++)
        {
            Db.Seed(new WorkOrderEntity { Code = $"WO{i:D3}", ProductName = $"产品{i}", LineId = 1, ProcessId = 1, PlannedQty = 100, Status = WorkOrderStatusEnum.Draft });
        }

        var query = new WorkOrderPageQuery { Page = 2, PageSize = 5 };

        var result = await WorkOrderService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        var data = result.Data;
        data!.Total.Should().Be(15);
        data.Items.Count.Should().Be(5);
    }

    [Fact]
    public async Task PageAsync_ShouldFilterByProductName()
    {
        Db.Seed(
            new WorkOrderEntity { Code = "WO001", ProductName = "手机", LineId = 1, ProcessId = 1, PlannedQty = 100, Status = WorkOrderStatusEnum.Draft },
            new WorkOrderEntity { Code = "WO002", ProductName = "电脑", LineId = 1, ProcessId = 1, PlannedQty = 200, Status = WorkOrderStatusEnum.Draft }
        );

        var query = new WorkOrderPageQuery { ProductName = "手机", Page = 1, PageSize = 10 };

        var result = await WorkOrderService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Total.Should().Be(1);
    }

    // ============================================================
    // GetAsync
    // ============================================================

    [Fact]
    public async Task GetAsync_ShouldReturnWorkOrder()
    {
        var wo = new WorkOrderEntity { Code = "WO001", ProductName = "产品A", LineId = 1, ProcessId = 1, PlannedQty = 100 };
        Db.Seed(wo);

        var result = await WorkOrderService.GetAsync(wo.Id);

        result.IsSuccess.Should().BeTrue();
        var dto = result.Data;
        dto.Should().NotBeNull();
        dto!.Code.Should().Be("WO001");
        dto.ProductName.Should().Be("产品A");
        dto.StatusName.Should().Be("草稿");
    }

    [Fact]
    public async Task GetAsync_NotFound_ShouldThrow()
    {
        var act = () => WorkOrderService.GetAsync(99999);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("工单不存在"));
    }

    // ============================================================
    // CreateAsync
    // ============================================================

    [Fact]
    public async Task CreateAsync_ShouldCreateWorkOrder()
    {
        var command = new WorkOrderCreateCommand
        {
            Code = "WO001",
            ProductName = "产品A",
            LineId = 1,
            ProcessId = 1,
            PlannedQty = 100,
            Priority = 5,
        };

        var result = await WorkOrderService.CreateAsync(command);

        result.IsSuccess.Should().BeTrue();
        var id = result.Data;
        id.Should().BeGreaterThan(0);

        var orders = await AllWorkOrdersAsync();
        orders.Should().ContainSingle();
        orders[0].Code.Should().Be("WO001");
        orders[0].Status.Should().Be(WorkOrderStatusEnum.Draft);
        orders[0].PlannedQty.Should().Be(100);
    }

    [Fact]
    public async Task CreateAsync_DuplicateCode_ShouldThrow()
    {
        Db.Seed(new WorkOrderEntity { Code = "WO001", ProductName = "已有工单", LineId = 1, ProcessId = 1, PlannedQty = 100 });

        var command = new WorkOrderCreateCommand { Code = "WO001", ProductName = "新工单", LineId = 1, ProcessId = 1, PlannedQty = 100 };

        var act = () => WorkOrderService.CreateAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("已存在"));
    }

    [Fact]
    public async Task CreateAsync_LineNotExists_ShouldThrow()
    {
        ReferenceDataServiceMock
            .Setup(s => s.LineExistsAsync(It.IsAny<long>()))
            .ReturnsAsync(false);

        var command = new WorkOrderCreateCommand { Code = "WO001", ProductName = "产品A", LineId = 999, ProcessId = 1, PlannedQty = 100 };

        var act = () => WorkOrderService.CreateAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("产线不存在"));
    }

    // ============================================================
    // UpdateAsync
    // ============================================================

    [Fact]
    public async Task UpdateAsync_ShouldUpdateWorkOrder()
    {
        var wo = new WorkOrderEntity { Code = "WO001", ProductName = "原名称", LineId = 1, ProcessId = 1, PlannedQty = 100, Status = WorkOrderStatusEnum.Draft };
        Db.Seed(wo);

        var command = new WorkOrderUpdateCommand
        {
            Id = wo.Id,
            ProductName = "新名称",
            LineId = 1,
            ProcessId = 1,
            PlannedQty = 200,
            Priority = 5,
        };

        var result = await WorkOrderService.UpdateAsync(command);

        result.IsSuccess.Should().BeTrue();
        var updated = await Client.Queryable<WorkOrderEntity>().FirstAsync(w => w.Id == wo.Id);
        updated.ProductName.Should().Be("新名称");
        updated.PlannedQty.Should().Be(200);
        updated.Priority.Should().Be(5);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ShouldThrow()
    {
        var command = new WorkOrderUpdateCommand { Id = 99999, ProductName = "不存在", LineId = 1, ProcessId = 1, PlannedQty = 100 };

        var act = () => WorkOrderService.UpdateAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("工单不存在"));
    }

    [Fact]
    public async Task UpdateAsync_NotDraft_ShouldThrow()
    {
        var wo = new WorkOrderEntity { Code = "WO001", ProductName = "产品", LineId = 1, ProcessId = 1, PlannedQty = 100, Status = WorkOrderStatusEnum.InProgress };
        Db.Seed(wo);

        var command = new WorkOrderUpdateCommand { Id = wo.Id, ProductName = "新名称", LineId = 1, ProcessId = 1, PlannedQty = 200 };

        var act = () => WorkOrderService.UpdateAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("草稿"));
    }

    [Fact]
    public async Task UpdateAsync_LineChangedAndNotExists_ShouldThrow()
    {
        var wo = new WorkOrderEntity { Code = "WO001", ProductName = "产品", LineId = 1, ProcessId = 1, PlannedQty = 100, Status = WorkOrderStatusEnum.Draft };
        Db.Seed(wo);

        ReferenceDataServiceMock
            .Setup(s => s.LineExistsAsync(999))
            .ReturnsAsync(false);

        var command = new WorkOrderUpdateCommand { Id = wo.Id, ProductName = "新名称", LineId = 999, ProcessId = 1, PlannedQty = 200 };

        var act = () => WorkOrderService.UpdateAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("产线不存在"));
    }

    // ============================================================
    // DeleteAsync
    // ============================================================

    [Fact]
    public async Task DeleteAsync_ShouldDeleteDraftWorkOrder()
    {
        var wo = new WorkOrderEntity { Code = "WO001", ProductName = "待删除", LineId = 1, ProcessId = 1, PlannedQty = 100, Status = WorkOrderStatusEnum.Draft };
        Db.Seed(wo);

        var result = await WorkOrderService.DeleteAsync(wo.Id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ShouldThrow()
    {
        var act = () => WorkOrderService.DeleteAsync(99999);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("工单不存在"));
    }

    [Fact]
    public async Task DeleteAsync_NotDraft_ShouldThrow()
    {
        var wo = new WorkOrderEntity { Code = "WO001", ProductName = "生产中", LineId = 1, ProcessId = 1, PlannedQty = 100, Status = WorkOrderStatusEnum.InProgress };
        Db.Seed(wo);

        var act = () => WorkOrderService.DeleteAsync(wo.Id);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("草稿"));
    }

    // ============================================================
    // BatchDeleteAsync
    // ============================================================

    [Fact]
    public async Task BatchDeleteAsync_ShouldBatchDeleteDraftWorkOrders()
    {
        var w1 = new WorkOrderEntity { Code = "WO001", ProductName = "工单一", LineId = 1, ProcessId = 1, PlannedQty = 100, Status = WorkOrderStatusEnum.Draft };
        var w2 = new WorkOrderEntity { Code = "WO002", ProductName = "工单二", LineId = 1, ProcessId = 1, PlannedQty = 200, Status = WorkOrderStatusEnum.Draft };
        var w3 = new WorkOrderEntity { Code = "WO003", ProductName = "工单三(非草稿)", LineId = 1, ProcessId = 1, PlannedQty = 300, Status = WorkOrderStatusEnum.InProgress };
        Db.Seed(w1, w2, w3);

        var result = await WorkOrderService.BatchDeleteAsync(new List<long> { w1.Id, w2.Id, w3.Id });

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(2); // Only 2 drafts deleted
    }

    [Fact]
    public async Task BatchDeleteAsync_EmptyList_ShouldThrow()
    {
        var act = () => WorkOrderService.BatchDeleteAsync(new List<long>());

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("选择"));
    }

    // ============================================================
    // DispatchAsync（草稿 → 已派发）
    // ============================================================

    [Fact]
    public async Task DispatchAsync_ShouldDispatchWorkOrder()
    {
        var wo = new WorkOrderEntity { Code = "WO001", ProductName = "产品A", LineId = 1, ProcessId = 1, PlannedQty = 100, Status = WorkOrderStatusEnum.Draft };
        Db.Seed(wo);

        var command = new WorkOrderDispatchCommand { Id = wo.Id };

        var result = await WorkOrderService.DispatchAsync(command);

        result.IsSuccess.Should().BeTrue();
        var updated = await Client.Queryable<WorkOrderEntity>().FirstAsync(w => w.Id == wo.Id);
        updated.Status.Should().Be(WorkOrderStatusEnum.Dispatched);
    }

    [Fact]
    public async Task DispatchAsync_NotFound_ShouldThrow()
    {
        var command = new WorkOrderDispatchCommand { Id = 99999 };

        var act = () => WorkOrderService.DispatchAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("工单不存在"));
    }

    [Fact]
    public async Task DispatchAsync_NotDraft_ShouldThrow()
    {
        var wo = new WorkOrderEntity { Code = "WO001", ProductName = "产品A", LineId = 1, ProcessId = 1, PlannedQty = 100, Status = WorkOrderStatusEnum.InProgress };
        Db.Seed(wo);

        var command = new WorkOrderDispatchCommand { Id = wo.Id };

        var act = () => WorkOrderService.DispatchAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("草稿"));
    }

    // ============================================================
    // StartAsync（已派发 → 生产中）
    // ============================================================

    [Fact]
    public async Task StartAsync_ShouldStartWorkOrder()
    {
        var wo = new WorkOrderEntity { Code = "WO001", ProductName = "产品A", LineId = 1, ProcessId = 1, PlannedQty = 100, Status = WorkOrderStatusEnum.Dispatched };
        Db.Seed(wo);

        var command = new WorkOrderStartCommand { Id = wo.Id };

        var result = await WorkOrderService.StartAsync(command);

        result.IsSuccess.Should().BeTrue();
        var updated = await Client.Queryable<WorkOrderEntity>().FirstAsync(w => w.Id == wo.Id);
        updated.Status.Should().Be(WorkOrderStatusEnum.InProgress);
        updated.ActualStart.Should().NotBeNull();
    }

    [Fact]
    public async Task StartAsync_NotFound_ShouldThrow()
    {
        var command = new WorkOrderStartCommand { Id = 99999 };

        var act = () => WorkOrderService.StartAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("工单不存在"));
    }

    [Fact]
    public async Task StartAsync_NotDispatched_ShouldThrow()
    {
        var wo = new WorkOrderEntity { Code = "WO001", ProductName = "产品A", LineId = 1, ProcessId = 1, PlannedQty = 100, Status = WorkOrderStatusEnum.Draft };
        Db.Seed(wo);

        var command = new WorkOrderStartCommand { Id = wo.Id };

        var act = () => WorkOrderService.StartAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("已派发"));
    }

    // ============================================================
    // CompleteAsync（生产中 → 已完成）
    // ============================================================

    [Fact]
    public async Task CompleteAsync_ShouldCompleteWorkOrder()
    {
        var wo = new WorkOrderEntity { Code = "WO001", ProductName = "产品A", LineId = 1, ProcessId = 1, PlannedQty = 100, Status = WorkOrderStatusEnum.InProgress };
        Db.Seed(wo);
        // Seed a work report so GetTotalQtyByWorkOrderIdAsync returns data
        Db.Seed(new WorkReportEntity { WorkOrderId = wo.Id, QualifiedQty = 90, DefectQty = 5, ReportTime = DateTime.Now });

        var command = new WorkOrderCompleteCommand { Id = wo.Id };

        var result = await WorkOrderService.CompleteAsync(command);

        result.IsSuccess.Should().BeTrue();
        var updated = await Client.Queryable<WorkOrderEntity>().FirstAsync(w => w.Id == wo.Id);
        updated.Status.Should().Be(WorkOrderStatusEnum.Completed);
        updated.CompletedQty.Should().Be(90);
        updated.DefectQty.Should().Be(5);
        updated.ActualEnd.Should().NotBeNull();
    }

    [Fact]
    public async Task CompleteAsync_NotFound_ShouldThrow()
    {
        var command = new WorkOrderCompleteCommand { Id = 99999 };

        var act = () => WorkOrderService.CompleteAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("工单不存在"));
    }

    [Fact]
    public async Task CompleteAsync_NotInProgress_ShouldThrow()
    {
        var wo = new WorkOrderEntity { Code = "WO001", ProductName = "产品A", LineId = 1, ProcessId = 1, PlannedQty = 100, Status = WorkOrderStatusEnum.Dispatched };
        Db.Seed(wo);

        var command = new WorkOrderCompleteCommand { Id = wo.Id };

        var act = () => WorkOrderService.CompleteAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("生产中"));
    }

    // ============================================================
    // CloseAsync（已完成 → 已关闭）
    // ============================================================

    [Fact]
    public async Task CloseAsync_ShouldCloseWorkOrder()
    {
        var wo = new WorkOrderEntity { Code = "WO001", ProductName = "产品A", LineId = 1, ProcessId = 1, PlannedQty = 100, Status = WorkOrderStatusEnum.Completed };
        Db.Seed(wo);

        var command = new WorkOrderCloseCommand { Id = wo.Id };

        var result = await WorkOrderService.CloseAsync(command);

        result.IsSuccess.Should().BeTrue();
        var updated = await Client.Queryable<WorkOrderEntity>().FirstAsync(w => w.Id == wo.Id);
        updated.Status.Should().Be(WorkOrderStatusEnum.Closed);
    }

    [Fact]
    public async Task CloseAsync_NotFound_ShouldThrow()
    {
        var command = new WorkOrderCloseCommand { Id = 99999 };

        var act = () => WorkOrderService.CloseAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("工单不存在"));
    }

    [Fact]
    public async Task CloseAsync_NotCompleted_ShouldThrow()
    {
        var wo = new WorkOrderEntity { Code = "WO001", ProductName = "产品A", LineId = 1, ProcessId = 1, PlannedQty = 100, Status = WorkOrderStatusEnum.InProgress };
        Db.Seed(wo);

        var command = new WorkOrderCloseCommand { Id = wo.Id };

        var act = () => WorkOrderService.CloseAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("已完成"));
    }

    // ============================================================
    // PauseAsync（生产中 → 已暂停）
    // ============================================================

    [Fact]
    public async Task PauseAsync_ShouldPauseWorkOrder()
    {
        var wo = new WorkOrderEntity { Code = "WO001", ProductName = "产品A", LineId = 1, ProcessId = 1, PlannedQty = 100, Status = WorkOrderStatusEnum.InProgress };
        Db.Seed(wo);

        var command = new WorkOrderPauseCommand { Id = wo.Id, Reason = "物料短缺" };

        var result = await WorkOrderService.PauseAsync(command);

        result.IsSuccess.Should().BeTrue();
        var updated = await Client.Queryable<WorkOrderEntity>().FirstAsync(w => w.Id == wo.Id);
        updated.Status.Should().Be(WorkOrderStatusEnum.Paused);
        updated.Remark.Should().Contain("物料短缺");
    }

    [Fact]
    public async Task PauseAsync_NotFound_ShouldThrow()
    {
        var command = new WorkOrderPauseCommand { Id = 99999 };

        var act = () => WorkOrderService.PauseAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("工单不存在"));
    }

    [Fact]
    public async Task PauseAsync_NotInProgress_ShouldThrow()
    {
        var wo = new WorkOrderEntity { Code = "WO001", ProductName = "产品A", LineId = 1, ProcessId = 1, PlannedQty = 100, Status = WorkOrderStatusEnum.Dispatched };
        Db.Seed(wo);

        var command = new WorkOrderPauseCommand { Id = wo.Id };

        var act = () => WorkOrderService.PauseAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("生产中"));
    }

    // ============================================================
    // CancelAsync（已派发 → 已取消）
    // ============================================================

    [Fact]
    public async Task CancelAsync_ShouldCancelWorkOrder()
    {
        var wo = new WorkOrderEntity { Code = "WO001", ProductName = "产品A", LineId = 1, ProcessId = 1, PlannedQty = 100, Status = WorkOrderStatusEnum.Dispatched };
        Db.Seed(wo);

        var command = new WorkOrderCancelCommand { Id = wo.Id, Reason = "计划变更" };

        var result = await WorkOrderService.CancelAsync(command);

        result.IsSuccess.Should().BeTrue();
        var updated = await Client.Queryable<WorkOrderEntity>().FirstAsync(w => w.Id == wo.Id);
        updated.Status.Should().Be(WorkOrderStatusEnum.Cancelled);
        updated.Remark.Should().Contain("计划变更");
    }

    [Fact]
    public async Task CancelAsync_NotFound_ShouldThrow()
    {
        var command = new WorkOrderCancelCommand { Id = 99999 };

        var act = () => WorkOrderService.CancelAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("工单不存在"));
    }

    [Fact]
    public async Task CancelAsync_NotDispatched_ShouldThrow()
    {
        var wo = new WorkOrderEntity { Code = "WO001", ProductName = "产品A", LineId = 1, ProcessId = 1, PlannedQty = 100, Status = WorkOrderStatusEnum.InProgress };
        Db.Seed(wo);

        var command = new WorkOrderCancelCommand { Id = wo.Id };

        var act = () => WorkOrderService.CancelAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("已派发"));
    }
}
