using FluentAssertions;
using Lzq.Core.Models;
using Lzq.MES.Application.Contracts.Commands;
using Lzq.MES.Application.Contracts.Dtos;
using Lzq.MES.Application.Contracts.Queries;
using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.Enums;
using Lzq.MES.Tests.Helpers;
using Xunit;

namespace Lzq.MES.Tests.Services;

[Collection("BaseData Tests")]
public class WorkshopServiceTests : ServiceTestBase
{
    private FactoryEntity _defaultFactory = null!;

    public WorkshopServiceTests()
    {
        // Seed a parent factory for all workshop tests
        _defaultFactory = new FactoryEntity { Code = "F001", Name = "默认工厂" };
        Db.Seed(_defaultFactory);
    }

    // ============================================================
    // PageAsync
    // ============================================================

    [Fact]
    public async Task PageAsync_ShouldReturnPagedWorkshops()
    {
        Db.Seed(
            new WorkshopEntity { Code = "W001", Name = "车间一", FactoryId = _defaultFactory.Id },
            new WorkshopEntity { Code = "W002", Name = "车间二", FactoryId = _defaultFactory.Id }
        );

        var query = new WorkshopPageQuery { Page = 1, PageSize = 10 };

        var result = await WorkshopService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Total.Should().Be(2);
    }

    [Fact]
    public async Task PageAsync_ShouldFilterByFactoryId()
    {
        Db.Seed(
            new WorkshopEntity { Code = "W001", Name = "车间一", FactoryId = _defaultFactory.Id },
            new WorkshopEntity { Code = "W002", Name = "车间二", FactoryId = 99999 }
        );

        var query = new WorkshopPageQuery { FactoryId = _defaultFactory.Id, Page = 1, PageSize = 10 };

        var result = await WorkshopService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Total.Should().Be(1);
    }

    // ============================================================
    // ListByFactoryAsync
    // ============================================================

    [Fact]
    public async Task ListByFactoryAsync_ShouldReturnEnabledWorkshops()
    {
        Db.Seed(
            new WorkshopEntity { Code = "W001", Name = "车间一", FactoryId = _defaultFactory.Id, Status = EnableStatusEnum.Enabled },
            new WorkshopEntity { Code = "W002", Name = "车间二", FactoryId = _defaultFactory.Id, Status = EnableStatusEnum.Disabled }
        );

        var result = await WorkshopService.ListByFactoryAsync(_defaultFactory.Id);

        result.IsSuccess.Should().BeTrue();
        var list = result.Data;
        list.Should().NotBeNull();
        list!.Count.Should().Be(1);
    }

    // ============================================================
    // TreeAsync
    // ============================================================

    [Fact]
    public async Task TreeAsync_ShouldReturnWorkshopLineTree()
    {
        var workshop = new WorkshopEntity { Code = "W001", Name = "车间一", FactoryId = _defaultFactory.Id };
        Db.Seed(workshop);

        var result = await WorkshopService.TreeAsync(_defaultFactory.Id);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task TreeAsync_WrongFactoryId_ShouldReturnEmpty()
    {
        var result = await WorkshopService.TreeAsync(99999);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Count.Should().Be(0);
    }

    // ============================================================
    // CreateAsync
    // ============================================================

    [Fact]
    public async Task CreateAsync_ShouldCreateWorkshop()
    {
        var command = new WorkshopCreateCommand
        {
            Code = "W001",
            Name = "新车间",
            FactoryId = _defaultFactory.Id,
            Manager = "张三",
        };

        var result = await WorkshopService.CreateAsync(command);

        result.IsSuccess.Should().BeTrue();
        var workshops = await AllWorkshopsAsync();
        workshops.Should().ContainSingle();
        workshops[0].Code.Should().Be("W001");
    }

    [Fact]
    public async Task CreateAsync_FactoryNotFound_ShouldThrow()
    {
        var command = new WorkshopCreateCommand
        {
            Code = "W001",
            Name = "新车间",
            FactoryId = 99999,
        };

        var act = () => WorkshopService.CreateAsync(command);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("工厂"));
    }

    [Fact]
    public async Task CreateAsync_DuplicateCode_ShouldThrow()
    {
        Db.Seed(new WorkshopEntity { Code = "W001", Name = "已有车间", FactoryId = _defaultFactory.Id });

        var command = new WorkshopCreateCommand
        {
            Code = "W001",
            Name = "新车间",
            FactoryId = _defaultFactory.Id,
        };

        var act = () => WorkshopService.CreateAsync(command);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("已存在"));
    }

    // ============================================================
    // UpdateAsync
    // ============================================================

    [Fact]
    public async Task UpdateAsync_ShouldUpdateWorkshop()
    {
        var workshop = new WorkshopEntity { Code = "W001", Name = "原名称", FactoryId = _defaultFactory.Id };
        Db.Seed(workshop);

        var command = new WorkshopUpdateCommand
        {
            Id = workshop.Id,
            Code = "W001",
            Name = "新名称",
            FactoryId = _defaultFactory.Id,
            Manager = "李四",
            Status = EnableStatusEnum.Disabled,
        };

        var result = await WorkshopService.UpdateAsync(command);

        result.IsSuccess.Should().BeTrue();
        var updated = await Client.Queryable<WorkshopEntity>().FirstAsync(w => w.Id == workshop.Id);
        updated.Name.Should().Be("新名称");
        updated.Manager.Should().Be("李四");
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ShouldThrow()
    {
        var command = new WorkshopUpdateCommand
        {
            Id = 99999,
            Code = "W999",
            Name = "不存在的车间",
            FactoryId = _defaultFactory.Id,
        };

        var act = () => WorkshopService.UpdateAsync(command);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("不存在"));
    }

    // ============================================================
    // DeleteAsync
    // ============================================================

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete()
    {
        var workshop = new WorkshopEntity { Code = "W001", Name = "待删除车间", FactoryId = _defaultFactory.Id };
        Db.Seed(workshop);

        var result = await WorkshopService.DeleteAsync(workshop.Id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ShouldThrow()
    {
        var act = () => WorkshopService.DeleteAsync(99999);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("不存在"));
    }

    // ============================================================
    // BatchDeleteAsync
    // ============================================================

    [Fact]
    public async Task BatchDeleteAsync_ShouldBatchSoftDelete()
    {
        var w1 = new WorkshopEntity { Code = "W001", Name = "车间一", FactoryId = _defaultFactory.Id };
        var w2 = new WorkshopEntity { Code = "W002", Name = "车间二", FactoryId = _defaultFactory.Id };
        Db.Seed(w1, w2);

        var result = await WorkshopService.BatchDeleteAsync(new List<long> { w1.Id, w2.Id });

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task BatchDeleteAsync_EmptyList_ShouldThrow()
    {
        var act = () => WorkshopService.BatchDeleteAsync(new List<long>());

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("选择"));
    }

    // ============================================================
    // Additional Field Tests
    // ============================================================

    [Fact]
    public async Task CreateAsync_ShouldPersistManager()
    {
        var command = new WorkshopCreateCommand
        {
            Code = "W001",
            Name = "车间一",
            FactoryId = _defaultFactory.Id,
            Manager = "张三",
        };

        var result = await WorkshopService.CreateAsync(command);

        result.IsSuccess.Should().BeTrue();
        var workshops = await AllWorkshopsAsync();
        workshops[0].Manager.Should().Be("张三");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateManager()
    {
        var workshop = new WorkshopEntity { Code = "W001", Name = "车间一", FactoryId = _defaultFactory.Id, Manager = "原负责人" };
        Db.Seed(workshop);

        var command = new WorkshopUpdateCommand
        {
            Id = workshop.Id,
            Code = "W001",
            Name = "车间一",
            FactoryId = _defaultFactory.Id,
            Manager = "新负责人李四",
        };

        var result = await WorkshopService.UpdateAsync(command);

        result.IsSuccess.Should().BeTrue();
        var updated = await Client.Queryable<WorkshopEntity>().FirstAsync(w => w.Id == workshop.Id);
        updated.Manager.Should().Be("新负责人李四");
    }

    [Fact]
    public async Task PageAsync_ShouldFilterByName()
    {
        Db.Seed(
            new WorkshopEntity { Code = "W001", Name = "装配车间", FactoryId = _defaultFactory.Id },
            new WorkshopEntity { Code = "W002", Name = "焊接车间", FactoryId = _defaultFactory.Id }
        );

        var query = new WorkshopPageQuery { Name = "装配", Page = 1, PageSize = 10 };

        var result = await WorkshopService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        var data = result.Data;
        data!.Total.Should().Be(1);
    }

    [Fact]
    public async Task PageAsync_ShouldFilterByStatus()
    {
        Db.Seed(
            new WorkshopEntity { Code = "W001", Name = "启用车间", FactoryId = _defaultFactory.Id, Status = EnableStatusEnum.Enabled },
            new WorkshopEntity { Code = "W002", Name = "禁用车间", FactoryId = _defaultFactory.Id, Status = EnableStatusEnum.Disabled }
        );

        var query = new WorkshopPageQuery { Status = EnableStatusEnum.Enabled, Page = 1, PageSize = 10 };

        var result = await WorkshopService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        var data = result.Data;
        data!.Total.Should().Be(1);
    }

    [Fact]
    public async Task PageAsync_ShouldPaginate()
    {
        for (int i = 1; i <= 15; i++)
        {
            Db.Seed(new WorkshopEntity { Code = $"W{i:D3}", Name = $"车间{i}", FactoryId = _defaultFactory.Id });
        }

        var query = new WorkshopPageQuery { Page = 2, PageSize = 5 };

        var result = await WorkshopService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        var data = result.Data;
        data!.Total.Should().Be(15);
        data.Items.Count.Should().Be(5);
    }

    [Fact]
    public async Task ListByFactoryAsync_EmptyFactory_ShouldReturnEmpty()
    {
        var result = await WorkshopService.ListByFactoryAsync(_defaultFactory.Id);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Count.Should().Be(0);
    }
}
