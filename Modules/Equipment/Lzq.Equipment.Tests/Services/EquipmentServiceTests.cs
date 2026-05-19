using FluentAssertions;
using Lzq.Equipment.Application.Contracts.Commands;
using Lzq.Equipment.Application.Contracts.Queries;
using Lzq.Equipment.Domain.Entities;
using Lzq.Equipment.Domain.Enums;
using Lzq.Equipment.Tests.Helpers;
using Xunit;

namespace Lzq.Equipment.Tests.Services;

[Collection("Equipment Tests")]
public class EquipmentServiceTests : ServiceTestBase
{
    // ============================================================
    // PageAsync
    // ============================================================

    [Fact]
    public async Task PageAsync_ShouldReturnPagedEquipments()
    {
        Db.Seed(
            new EquipmentEntity { Code = "EQ001", Name = "设备一", EquipmentType = EquipmentTypeEnum.Production },
            new EquipmentEntity { Code = "EQ002", Name = "设备二", EquipmentType = EquipmentTypeEnum.Testing }
        );

        var query = new EquipmentPageQuery { Page = 1, PageSize = 10 };

        var result = await EquipmentService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Total.Should().Be(2);
    }

    [Fact]
    public async Task PageAsync_ShouldFilterByCode()
    {
        Db.Seed(
            new EquipmentEntity { Code = "EQ001", Name = "设备一" },
            new EquipmentEntity { Code = "EQ002", Name = "设备二" }
        );

        var query = new EquipmentPageQuery { Code = "001", Page = 1, PageSize = 10 };

        var result = await EquipmentService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Total.Should().Be(1);
    }

    [Fact]
    public async Task PageAsync_ShouldFilterByStatus()
    {
        Db.Seed(
            new EquipmentEntity { Code = "EQ001", Name = "设备一", Status = EquipmentStatusEnum.Normal },
            new EquipmentEntity { Code = "EQ002", Name = "设备二", Status = EquipmentStatusEnum.Stopped }
        );

        var query = new EquipmentPageQuery { Status = EquipmentStatusEnum.Stopped, Page = 1, PageSize = 10 };

        var result = await EquipmentService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Total.Should().Be(1);
    }

    [Fact]
    public async Task PageAsync_ShouldPaginate()
    {
        for (int i = 1; i <= 15; i++)
        {
            Db.Seed(new EquipmentEntity { Code = $"EQ{i:D3}", Name = $"设备{i}", EquipmentType = EquipmentTypeEnum.Production });
        }

        var query = new EquipmentPageQuery { Page = 2, PageSize = 5 };

        var result = await EquipmentService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        var data = result.Data;
        data!.Total.Should().Be(15);
        data.Items.Count.Should().Be(5);
    }

    [Fact]
    public async Task PageAsync_ShouldFilterByName()
    {
        Db.Seed(
            new EquipmentEntity { Code = "EQ001", Name = "焊接设备", EquipmentType = EquipmentTypeEnum.Production },
            new EquipmentEntity { Code = "EQ002", Name = "切割设备", EquipmentType = EquipmentTypeEnum.Production }
        );

        var query = new EquipmentPageQuery { Name = "焊接", Page = 1, PageSize = 10 };

        var result = await EquipmentService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Total.Should().Be(1);
    }

    // ============================================================
    // GetAsync
    // ============================================================

    [Fact]
    public async Task GetAsync_ShouldReturnEquipment()
    {
        var equipment = new EquipmentEntity { Code = "EQ001", Name = "测试设备", EquipmentType = EquipmentTypeEnum.Production };
        Db.Seed(equipment);

        var result = await EquipmentService.GetAsync(equipment.Id);

        result.IsSuccess.Should().BeTrue();
        var dto = result.Data;
        dto.Should().NotBeNull();
        dto!.Code.Should().Be("EQ001");
        dto.Name.Should().Be("测试设备");
        dto.EquipmentTypeName.Should().Be("生产设备");
    }

    [Fact]
    public async Task GetAsync_NotFound_ShouldThrow()
    {
        var act = () => EquipmentService.GetAsync(99999);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("设备不存在"));
    }

    // ============================================================
    // CreateAsync
    // ============================================================

    [Fact]
    public async Task CreateAsync_ShouldCreateEquipment()
    {
        var command = new EquipmentCreateCommand
        {
            Code = "EQ001",
            Name = "新设备",
            EquipmentType = EquipmentTypeEnum.Production,
            Spec = "型号A",
            LineId = 1,
            LineName = "产线一",
        };

        var result = await EquipmentService.CreateAsync(command);

        result.IsSuccess.Should().BeTrue();
        var id = result.Data;
        id.Should().BeGreaterThan(0);

        var equipments = await AllEquipmentsAsync();
        equipments.Should().ContainSingle();
        equipments[0].Code.Should().Be("EQ001");
        equipments[0].Status.Should().Be(EquipmentStatusEnum.Normal);
    }

    [Fact]
    public async Task CreateAsync_DuplicateCode_ShouldThrow()
    {
        Db.Seed(new EquipmentEntity { Code = "EQ001", Name = "已有设备" });

        var command = new EquipmentCreateCommand { Code = "EQ001", Name = "新设备" };

        var act = () => EquipmentService.CreateAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("已存在"));
    }

    // ============================================================
    // UpdateAsync
    // ============================================================

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEquipment()
    {
        var equipment = new EquipmentEntity { Code = "EQ001", Name = "原名称", EquipmentType = EquipmentTypeEnum.Production };
        Db.Seed(equipment);

        var command = new EquipmentUpdateCommand
        {
            Id = equipment.Id,
            Name = "新名称",
            Spec = "新型号",
            Location = "A区",
        };

        var result = await EquipmentService.UpdateAsync(command);

        result.IsSuccess.Should().BeTrue();
        var updated = await Client.Queryable<EquipmentEntity>().FirstAsync(e => e.Id == equipment.Id);
        updated.Name.Should().Be("新名称");
        updated.Spec.Should().Be("新型号");
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ShouldThrow()
    {
        var command = new EquipmentUpdateCommand { Id = 99999, Name = "不存在的设备" };

        var act = () => EquipmentService.UpdateAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("设备不存在"));
    }

    // ============================================================
    // DeleteAsync
    // ============================================================

    [Fact]
    public async Task DeleteAsync_ShouldDeleteEquipment()
    {
        var equipment = new EquipmentEntity { Code = "EQ001", Name = "待删除设备" };
        Db.Seed(equipment);

        var result = await EquipmentService.DeleteAsync(equipment.Id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ShouldThrow()
    {
        var act = () => EquipmentService.DeleteAsync(99999);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("设备不存在"));
    }

    // ============================================================
    // UpdateStatusAsync
    // ============================================================

    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdateStatus()
    {
        var equipment = new EquipmentEntity { Code = "EQ001", Name = "测试设备", Status = EquipmentStatusEnum.Normal };
        Db.Seed(equipment);

        var command = new EquipmentUpdateStatusCommand { Id = equipment.Id, Status = EquipmentStatusEnum.Stopped };

        var result = await EquipmentService.UpdateStatusAsync(command);

        result.IsSuccess.Should().BeTrue();
        var updated = await Client.Queryable<EquipmentEntity>().FirstAsync(e => e.Id == equipment.Id);
        updated.Status.Should().Be(EquipmentStatusEnum.Stopped);
    }

    [Fact]
    public async Task UpdateStatusAsync_NotFound_ShouldThrow()
    {
        var command = new EquipmentUpdateStatusCommand { Id = 99999, Status = EquipmentStatusEnum.Stopped };

        var act = () => EquipmentService.UpdateStatusAsync(command);

        await act.Should().ThrowAsync<UserFriendlyException>()
            .Where(e => e.Message.Contains("设备不存在"));
    }

    // ============================================================
    // GetStatisticsAsync
    // ============================================================

    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnStatistics()
    {
        Db.Seed(
            new EquipmentEntity { Code = "EQ001", Name = "生产设备", EquipmentType = EquipmentTypeEnum.Production, Status = EquipmentStatusEnum.Normal },
            new EquipmentEntity { Code = "EQ002", Name = "检测设备", EquipmentType = EquipmentTypeEnum.Testing, Status = EquipmentStatusEnum.UnderRepair },
            new EquipmentEntity { Code = "EQ003", Name = "辅助设备", EquipmentType = EquipmentTypeEnum.Auxiliary, Status = EquipmentStatusEnum.Stopped }
        );

        var result = await EquipmentService.GetStatisticsAsync();

        result.IsSuccess.Should().BeTrue();
        var stats = result.Data;
        stats.Should().NotBeNull();
        stats!.TotalCount.Should().Be(3);
        stats.NormalCount.Should().Be(1);
        stats.StoppedCount.Should().Be(1);
        stats.ProductionCount.Should().Be(1);
        stats.TestingCount.Should().Be(1);
        stats.AuxiliaryCount.Should().Be(1);
    }
}
