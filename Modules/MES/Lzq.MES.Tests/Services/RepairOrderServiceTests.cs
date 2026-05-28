using FluentAssertions;
using Lzq.MES.Application.Contracts.Commands;
using Lzq.MES.Application.Contracts.Queries;
using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.Enums;
using Lzq.MES.Tests.Helpers;
using Xunit;

namespace Lzq.MES.Tests.Services;

[Collection("Equipment Tests")]
public class RepairOrderServiceTests : ServiceTestBase
{
    // ============================================================
    // PageAsync
    // ============================================================

    [Fact]
    public async Task PageAsync_ShouldReturnPagedRepairOrders()
    {
        Db.Seed(
            new RepairOrderEntity { Code = "BX001", Description = "故障一", EquipmentId = 1, Status = RepairStatusEnum.Pending },
            new RepairOrderEntity { Code = "BX002", Description = "故障二", EquipmentId = 2, Status = RepairStatusEnum.Completed }
        );

        var query = new RepairOrderPageQuery { Page = 1, PageSize = 10 };

        var result = await RepairOrderService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Total.Should().Be(2);
    }

    [Fact]
    public async Task PageAsync_ShouldFilterByStatus()
    {
        Db.Seed(
            new RepairOrderEntity { Code = "BX001", Description = "待派工", EquipmentId = 1, Status = RepairStatusEnum.Pending },
            new RepairOrderEntity { Code = "BX002", Description = "已完成", EquipmentId = 2, Status = RepairStatusEnum.Completed }
        );

        var query = new RepairOrderPageQuery { Status = RepairStatusEnum.Pending, Page = 1, PageSize = 10 };

        var result = await RepairOrderService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Total.Should().Be(1);
    }

    [Fact]
    public async Task PageAsync_ShouldPaginate()
    {
        for (int i = 1; i <= 15; i++)
        {
            Db.Seed(new RepairOrderEntity { Code = $"BX{i:D3}", Description = $"故障{i}", EquipmentId = i, Status = RepairStatusEnum.Pending });
        }

        var query = new RepairOrderPageQuery { Page = 2, PageSize = 5 };

        var result = await RepairOrderService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        var data = result.Data;
        data!.Total.Should().Be(15);
        data.Items.Count.Should().Be(5);
    }

    // ============================================================
    // GetAsync
    // ============================================================

    [Fact]
    public async Task GetAsync_ShouldReturnRepairOrder()
    {
        var order = new RepairOrderEntity { Code = "BX001", Description = "故障描述", EquipmentId = 1, Status = RepairStatusEnum.Pending };
        Db.Seed(order);

        var result = await RepairOrderService.GetAsync(order.Id);

        result.IsSuccess.Should().BeTrue();
        var dto = result.Data;
        dto.Should().NotBeNull();
        dto!.Code.Should().Be("BX001");
        dto.Description.Should().Be("故障描述");
        dto.StatusName.Should().Be("待派工");
    }

    [Fact]
    public async Task GetAsync_NotFound_ShouldThrow()
    {
        var act = () => RepairOrderService.GetAsync(99999);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("报修单不存在"));
    }

    // ============================================================
    // CreateAsync
    // ============================================================

    [Fact]
    public async Task CreateAsync_ShouldCreateRepairOrder()
    {
        var equipment = new EquipmentEntity { Code = "EQ001", Name = "测试设备", Status = EquipmentStatusEnum.Normal };
        Db.Seed(equipment);

        var command = new RepairOrderCreateCommand
        {
            EquipmentId = equipment.Id,
            EquipmentCode = "EQ001",
            EquipmentName = "测试设备",
            RepairType = 1,
            Description = "设备故障报修",
            Priority = RepairPriorityEnum.High,
        };

        var result = await RepairOrderService.CreateAsync(command);

        result.IsSuccess.Should().BeTrue();
        var id = result.Data;
        id.Should().BeGreaterThan(0);

        var orders = await AllRepairOrdersAsync();
        orders.Should().ContainSingle();
        orders[0].Status.Should().Be(RepairStatusEnum.Pending);
    }

    [Fact]
    public async Task CreateAsync_ShouldSetEquipmentStatusToUnderRepair()
    {
        var equipment = new EquipmentEntity { Code = "EQ001", Name = "测试设备", Status = EquipmentStatusEnum.Normal };
        Db.Seed(equipment);

        var command = new RepairOrderCreateCommand
        {
            EquipmentId = equipment.Id,
            EquipmentCode = "EQ001",
            Description = "故障报修",
        };

        await RepairOrderService.CreateAsync(command);

        var updatedEquipment = await Client.Queryable<EquipmentEntity>().FirstAsync(e => e.Id == equipment.Id);
        updatedEquipment.Status.Should().Be(EquipmentStatusEnum.UnderRepair);
    }

    // ============================================================
    // AssignAsync
    // ============================================================

    [Fact]
    public async Task AssignAsync_ShouldAssignRepairUser()
    {
        var order = new RepairOrderEntity { Code = "BX001", Description = "故障", EquipmentId = 1, Status = RepairStatusEnum.Pending };
        Db.Seed(order);

        var command = new RepairAssignCommand { Id = order.Id, RepairUserId = 100, RepairUserName = "维修工张三" };

        var result = await RepairOrderService.AssignAsync(command);

        result.IsSuccess.Should().BeTrue();
        var updated = await Client.Queryable<RepairOrderEntity>().FirstAsync(r => r.Id == order.Id);
        updated.Status.Should().Be(RepairStatusEnum.Assigned);
        updated.RepairUserId.Should().Be(100);
        updated.RepairUserName.Should().Be("维修工张三");
    }

    [Fact]
    public async Task AssignAsync_NotFound_ShouldThrow()
    {
        var command = new RepairAssignCommand { Id = 99999, RepairUserId = 100, RepairUserName = "张三" };

        var act = () => RepairOrderService.AssignAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("报修单不存在"));
    }

    [Fact]
    public async Task AssignAsync_NotPending_ShouldThrow()
    {
        var order = new RepairOrderEntity { Code = "BX001", Description = "故障", EquipmentId = 1, Status = RepairStatusEnum.InProgress };
        Db.Seed(order);

        var command = new RepairAssignCommand { Id = order.Id, RepairUserId = 100, RepairUserName = "张三" };

        var act = () => RepairOrderService.AssignAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("待派工"));
    }

    // ============================================================
    // StartAsync
    // ============================================================

    [Fact]
    public async Task StartAsync_ShouldStartRepair()
    {
        var order = new RepairOrderEntity { Code = "BX001", Description = "故障", EquipmentId = 1, Status = RepairStatusEnum.Assigned };
        Db.Seed(order);

        var command = new RepairStartCommand { Id = order.Id };

        var result = await RepairOrderService.StartAsync(command);

        result.IsSuccess.Should().BeTrue();
        var updated = await Client.Queryable<RepairOrderEntity>().FirstAsync(r => r.Id == order.Id);
        updated.Status.Should().Be(RepairStatusEnum.InProgress);
        updated.RepairStartTime.Should().NotBeNull();
    }

    [Fact]
    public async Task StartAsync_NotFound_ShouldThrow()
    {
        var command = new RepairStartCommand { Id = 99999 };

        var act = () => RepairOrderService.StartAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("报修单不存在"));
    }

    [Fact]
    public async Task StartAsync_NotAssigned_ShouldThrow()
    {
        var order = new RepairOrderEntity { Code = "BX001", Description = "故障", EquipmentId = 1, Status = RepairStatusEnum.Pending };
        Db.Seed(order);

        var command = new RepairStartCommand { Id = order.Id };

        var act = () => RepairOrderService.StartAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("已派工"));
    }

    // ============================================================
    // CompleteAsync
    // ============================================================

    [Fact]
    public async Task CompleteAsync_ShouldCompleteRepair()
    {
        var order = new RepairOrderEntity { Code = "BX001", Description = "故障", EquipmentId = 1, Status = RepairStatusEnum.InProgress };
        Db.Seed(order);

        var command = new RepairCompleteCommand
        {
            Id = order.Id,
            FaultReason = "轴承磨损",
            RepairProcess = "更换轴承",
            PartsUsed = "[\"轴承6205\"]",
            WorkHours = 2.5m,
            Cost = 300m,
        };

        var result = await RepairOrderService.CompleteAsync(command);

        result.IsSuccess.Should().BeTrue();
        var updated = await Client.Queryable<RepairOrderEntity>().FirstAsync(r => r.Id == order.Id);
        updated.Status.Should().Be(RepairStatusEnum.Completed);
        updated.FaultReason.Should().Be("轴承磨损");
        updated.RepairProcess.Should().Be("更换轴承");
        updated.WorkHours.Should().Be(2.5m);
        updated.Cost.Should().Be(300m);
    }

    [Fact]
    public async Task CompleteAsync_NotFound_ShouldThrow()
    {
        var command = new RepairCompleteCommand { Id = 99999 };

        var act = () => RepairOrderService.CompleteAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("报修单不存在"));
    }

    [Fact]
    public async Task CompleteAsync_NotInProgress_ShouldThrow()
    {
        var order = new RepairOrderEntity { Code = "BX001", Description = "故障", EquipmentId = 1, Status = RepairStatusEnum.Assigned };
        Db.Seed(order);

        var command = new RepairCompleteCommand { Id = order.Id };

        var act = () => RepairOrderService.CompleteAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("维修中"));
    }

    // ============================================================
    // AcceptAsync
    // ============================================================

    [Fact]
    public async Task AcceptAsync_ShouldAcceptRepair()
    {
        var equipment = new EquipmentEntity { Code = "EQ001", Name = "测试设备", Status = EquipmentStatusEnum.UnderRepair };
        Db.Seed(equipment);
        var order = new RepairOrderEntity { Code = "BX001", Description = "故障", EquipmentId = equipment.Id, Status = RepairStatusEnum.Completed };
        Db.Seed(order);

        var command = new RepairAcceptCommand { Id = order.Id, AcceptComment = "维修合格" };

        var result = await RepairOrderService.AcceptAsync(command);

        result.IsSuccess.Should().BeTrue();
        var updated = await Client.Queryable<RepairOrderEntity>().FirstAsync(r => r.Id == order.Id);
        updated.Status.Should().Be(RepairStatusEnum.Accepted);
        updated.AcceptComment.Should().Be("维修合格");
        updated.AcceptTime.Should().NotBeNull();
    }

    [Fact]
    public async Task AcceptAsync_NotFound_ShouldThrow()
    {
        var command = new RepairAcceptCommand { Id = 99999 };

        var act = () => RepairOrderService.AcceptAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("报修单不存在"));
    }

    [Fact]
    public async Task AcceptAsync_NotCompleted_ShouldThrow()
    {
        var order = new RepairOrderEntity { Code = "BX001", Description = "故障", EquipmentId = 1, Status = RepairStatusEnum.InProgress };
        Db.Seed(order);

        var command = new RepairAcceptCommand { Id = order.Id };

        var act = () => RepairOrderService.AcceptAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("已完工"));
    }

    // ============================================================
    // CancelAsync
    // ============================================================

    [Fact]
    public async Task CancelAsync_ShouldCancelRepairOrder()
    {
        var order = new RepairOrderEntity { Code = "BX001", Description = "故障", EquipmentId = 1, Status = RepairStatusEnum.Pending };
        Db.Seed(order);

        var result = await RepairOrderService.CancelAsync(order.Id);

        result.IsSuccess.Should().BeTrue();
        var updated = await Client.Queryable<RepairOrderEntity>().FirstAsync(r => r.Id == order.Id);
        updated.Status.Should().Be(RepairStatusEnum.Cancelled);
    }

    [Fact]
    public async Task CancelAsync_NotFound_ShouldThrow()
    {
        var act = () => RepairOrderService.CancelAsync(99999);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("报修单不存在"));
    }

    [Fact]
    public async Task CancelAsync_AlreadyAccepted_ShouldThrow()
    {
        var order = new RepairOrderEntity { Code = "BX001", Description = "故障", EquipmentId = 1, Status = RepairStatusEnum.Accepted };
        Db.Seed(order);

        var act = () => RepairOrderService.CancelAsync(order.Id);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("已验收"));
    }

    // ============================================================
    // GetStatisticsAsync
    // ============================================================

    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnStatistics()
    {
        Db.Seed(
            new RepairOrderEntity { Code = "BX001", Description = "故障一", EquipmentId = 1, Status = RepairStatusEnum.Pending, Priority = RepairPriorityEnum.Urgent },
            new RepairOrderEntity { Code = "BX002", Description = "故障二", EquipmentId = 2, Status = RepairStatusEnum.InProgress, Priority = RepairPriorityEnum.High },
            new RepairOrderEntity { Code = "BX003", Description = "故障三", EquipmentId = 3, Status = RepairStatusEnum.Accepted, Priority = RepairPriorityEnum.Medium, WorkHours = 5m, Cost = 500m }
        );

        var result = await RepairOrderService.GetStatisticsAsync(null, null);

        result.IsSuccess.Should().BeTrue();
        var stats = result.Data;
        stats.Should().NotBeNull();
        stats!.TotalCount.Should().Be(3);
        stats.PendingCount.Should().Be(1);        // Pending
        stats.InProgressCount.Should().Be(1);      // InProgress + Assigned（根据仓储逻辑）
        stats.CompletedCount.Should().Be(1);       // Completed + Accepted（根据仓储逻辑）
        stats.UrgentCount.Should().Be(1);          // Urgent
    }
}
