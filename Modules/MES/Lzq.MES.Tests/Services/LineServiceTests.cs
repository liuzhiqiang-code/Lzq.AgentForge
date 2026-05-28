using FluentAssertions;
using Lzq.MES.Application.Contracts.Commands;
using Lzq.MES.Application.Contracts.Dtos;
using Lzq.MES.Application.Contracts.Queries;
using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.Enums;
using Lzq.MES.Tests.Helpers;
using Lzq.Core.Models;
using Xunit;

namespace Lzq.MES.Tests.Services;

[Collection("BaseData Tests")]
public class LineServiceTests : ServiceTestBase
{
    private WorkshopEntity _defaultWorkshop = null!;

    public LineServiceTests()
    {
        var factory = new FactoryEntity { Code = "F001", Name = "默认工厂" };
        _defaultWorkshop = new WorkshopEntity { Code = "W001", Name = "默认车间", FactoryId = factory.Id };
        Db.Seed(factory, _defaultWorkshop);
    }

    // ============================================================
    // GetByIdAsync
    // ============================================================

    [Fact]
    public async Task GetByIdAsync_ShouldReturnLine()
    {
        var line = new LineEntity { Code = "L001", Name = "产线一", WorkshopId = _defaultWorkshop.Id };
        Db.Seed(line);

        var result = await LineService.GetByIdAsync(line.Id);

        result.Should().NotBeNull();
        result!.Code.Should().Be("L001");
        result.Name.Should().Be("产线一");
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ShouldReturnNull()
    {
        var result = await LineService.GetByIdAsync(99999);

        result.Should().BeNull();
    }

    // ============================================================
    // PageAsync
    // ============================================================

    [Fact]
    public async Task PageAsync_ShouldReturnPagedLines()
    {
        Db.Seed(
            new LineEntity { Code = "L001", Name = "产线一", WorkshopId = _defaultWorkshop.Id },
            new LineEntity { Code = "L002", Name = "产线二", WorkshopId = _defaultWorkshop.Id }
        );

        var query = new LinePageQuery { Page = 1, PageSize = 10 };

        var result = await LineService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Total.Should().Be(2);
    }

    [Fact]
    public async Task PageAsync_ShouldFilterByWorkshopId()
    {
        Db.Seed(
            new LineEntity { Code = "L001", Name = "产线一", WorkshopId = _defaultWorkshop.Id },
            new LineEntity { Code = "L002", Name = "产线二", WorkshopId = 99999 }
        );

        var query = new LinePageQuery { WorkshopId = _defaultWorkshop.Id, Page = 1, PageSize = 10 };

        var result = await LineService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Total.Should().Be(1);
    }

    // ============================================================
    // ListByWorkshopAsync
    // ============================================================

    [Fact]
    public async Task ListByWorkshopAsync_ShouldReturnEnabledLines()
    {
        Db.Seed(
            new LineEntity { Code = "L001", Name = "产线一", WorkshopId = _defaultWorkshop.Id, Status = EnableStatusEnum.Enabled },
            new LineEntity { Code = "L002", Name = "产线二", WorkshopId = _defaultWorkshop.Id, Status = EnableStatusEnum.Disabled }
        );

        var result = await LineService.ListByWorkshopAsync(_defaultWorkshop.Id);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Count.Should().Be(1);
    }

    // ============================================================
    // CreateAsync
    // ============================================================

    [Fact]
    public async Task CreateAsync_ShouldCreateLine()
    {
        var command = new LineCreateCommand
        {
            Code = "L001",
            Name = "新产线",
            WorkshopId = _defaultWorkshop.Id,
        };

        var result = await LineService.CreateAsync(command);

        result.IsSuccess.Should().BeTrue();
        var lines = await AllLinesAsync();
        lines.Should().ContainSingle();
        lines[0].Code.Should().Be("L001");
    }

    [Fact]
    public async Task CreateAsync_WorkshopNotFound_ShouldThrow()
    {
        var command = new LineCreateCommand
        {
            Code = "L001",
            Name = "新产线",
            WorkshopId = 99999,
        };

        var act = () => LineService.CreateAsync(command);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("车间"));
    }

    [Fact]
    public async Task CreateAsync_DuplicateCode_ShouldThrow()
    {
        Db.Seed(new LineEntity { Code = "L001", Name = "已有产线", WorkshopId = _defaultWorkshop.Id });

        var command = new LineCreateCommand
        {
            Code = "L001",
            Name = "新产线",
            WorkshopId = _defaultWorkshop.Id,
        };

        var act = () => LineService.CreateAsync(command);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("已存在"));
    }

    // ============================================================
    // UpdateAsync
    // ============================================================

    [Fact]
    public async Task UpdateAsync_ShouldUpdateLine()
    {
        var line = new LineEntity { Code = "L001", Name = "原名称", WorkshopId = _defaultWorkshop.Id };
        Db.Seed(line);

        var command = new LineUpdateCommand
        {
            Id = line.Id,
            Code = "L001",
            Name = "新名称",
            WorkshopId = _defaultWorkshop.Id,
            Status = EnableStatusEnum.Disabled,
        };

        var result = await LineService.UpdateAsync(command);

        result.IsSuccess.Should().BeTrue();
        var updated = await Client.Queryable<LineEntity>().FirstAsync(l => l.Id == line.Id);
        updated.Name.Should().Be("新名称");
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ShouldThrow()
    {
        var command = new LineUpdateCommand
        {
            Id = 99999,
            Code = "L999",
            Name = "不存在的产线",
            WorkshopId = _defaultWorkshop.Id,
        };

        var act = () => LineService.UpdateAsync(command);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("不存在"));
    }

    // ============================================================
    // DeleteAsync
    // ============================================================

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete()
    {
        var line = new LineEntity { Code = "L001", Name = "待删除产线", WorkshopId = _defaultWorkshop.Id };
        Db.Seed(line);

        var result = await LineService.DeleteAsync(line.Id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ShouldThrow()
    {
        var act = () => LineService.DeleteAsync(99999);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("不存在"));
    }

    // ============================================================
    // BatchDeleteAsync
    // ============================================================

    [Fact]
    public async Task BatchDeleteAsync_ShouldBatchSoftDelete()
    {
        var l1 = new LineEntity { Code = "L001", Name = "产线一", WorkshopId = _defaultWorkshop.Id };
        var l2 = new LineEntity { Code = "L002", Name = "产线二", WorkshopId = _defaultWorkshop.Id };
        Db.Seed(l1, l2);

        var result = await LineService.BatchDeleteAsync(new List<long> { l1.Id, l2.Id });

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task BatchDeleteAsync_EmptyList_ShouldThrow()
    {
        var act = () => LineService.BatchDeleteAsync(new List<long>());

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("选择"));
    }

    // ============================================================
    // Additional Field Tests
    // ============================================================

    [Fact]
    public async Task PageAsync_ShouldFilterByName()
    {
        Db.Seed(
            new LineEntity { Code = "L001", Name = "自动产线", WorkshopId = _defaultWorkshop.Id },
            new LineEntity { Code = "L002", Name = "手动产线", WorkshopId = _defaultWorkshop.Id }
        );

        var query = new LinePageQuery { Name = "自动", Page = 1, PageSize = 10 };

        var result = await LineService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        var data = result.Data;
        data!.Total.Should().Be(1);
    }

    [Fact]
    public async Task PageAsync_ShouldFilterByStatus()
    {
        Db.Seed(
            new LineEntity { Code = "L001", Name = "启用产线", WorkshopId = _defaultWorkshop.Id, Status = EnableStatusEnum.Enabled },
            new LineEntity { Code = "L002", Name = "禁用产线", WorkshopId = _defaultWorkshop.Id, Status = EnableStatusEnum.Disabled }
        );

        var query = new LinePageQuery { Status = EnableStatusEnum.Enabled, Page = 1, PageSize = 10 };

        var result = await LineService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        var data = result.Data;
        data!.Total.Should().Be(1);
    }

    [Fact]
    public async Task PageAsync_ShouldPaginate()
    {
        for (int i = 1; i <= 15; i++)
        {
            Db.Seed(new LineEntity { Code = $"L{i:D3}", Name = $"产线{i}", WorkshopId = _defaultWorkshop.Id });
        }

        var query = new LinePageQuery { Page = 2, PageSize = 5 };

        var result = await LineService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        var data = result.Data;
        data!.Total.Should().Be(15);
        data.Items.Count.Should().Be(5);
    }

    [Fact]
    public async Task ListByWorkshopAsync_EmptyWorkshop_ShouldReturnEmpty()
    {
        var result = await LineService.ListByWorkshopAsync(_defaultWorkshop.Id);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Count.Should().Be(0);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateStatus()
    {
        var line = new LineEntity { Code = "L001", Name = "产线一", WorkshopId = _defaultWorkshop.Id, Status = EnableStatusEnum.Enabled };
        Db.Seed(line);

        var command = new LineUpdateCommand
        {
            Id = line.Id,
            Code = "L001",
            Name = "产线一",
            WorkshopId = _defaultWorkshop.Id,
            Status = EnableStatusEnum.Disabled,
        };

        var result = await LineService.UpdateAsync(command);

        result.IsSuccess.Should().BeTrue();
        var updated = await Client.Queryable<LineEntity>().FirstAsync(l => l.Id == line.Id);
        updated.Status.Should().Be(EnableStatusEnum.Disabled);
    }
}
