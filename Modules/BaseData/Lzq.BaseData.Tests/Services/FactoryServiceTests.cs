using FluentAssertions;
using Lzq.BaseData.Application.Contracts.Commands;
using Lzq.BaseData.Application.Contracts.Dtos;
using Lzq.BaseData.Application.Contracts.Queries;
using Lzq.BaseData.Domain.Entities;
using Lzq.BaseData.Domain.Enums;
using Lzq.BaseData.Tests.Helpers;
using Lzq.Core.Models;
using Xunit;

namespace Lzq.BaseData.Tests.Services;

[Collection("BaseData Tests")]
public class FactoryServiceTests : ServiceTestBase
{
    // ============================================================
    // PageAsync
    // ============================================================

    [Fact]
    public async Task PageAsync_ShouldReturnPagedFactories()
    {
        // Arrange
        Db.Seed(
            new FactoryEntity { Code = "F001", Name = "工厂一", Status = EnableStatusEnum.Enabled },
            new FactoryEntity { Code = "F002", Name = "工厂二", Status = EnableStatusEnum.Enabled },
            new FactoryEntity { Code = "F003", Name = "工厂三", Status = EnableStatusEnum.Disabled }
        );

        var query = new FactoryPageQuery { Page = 1, PageSize = 10 };

        // Act
        var result = await FactoryService.PageAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var data = result.Data;
        data.Should().NotBeNull();
        data!.Total.Should().Be(3);
    }

    [Fact]
    public async Task PageAsync_ShouldFilterByCode()
    {
        Db.Seed(
            new FactoryEntity { Code = "F001", Name = "工厂一" },
            new FactoryEntity { Code = "F002", Name = "工厂二" }
        );

        var query = new FactoryPageQuery { Code = "F001", Page = 1, PageSize = 10 };

        var result = await FactoryService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        var data = result.Data;
        data!.Total.Should().Be(1);
    }

    // ============================================================
    // TreeAsync
    // ============================================================

    [Fact]
    public async Task TreeAsync_ShouldReturnFullTree()
    {
        var factory = new FactoryEntity { Code = "F001", Name = "工厂一" };
        Db.Seed(factory);

        var result = await FactoryService.TreeAsync();

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task TreeAsync_EmptyDatabase_ShouldReturnEmptyList()
    {
        var result = await FactoryService.TreeAsync();

        result.IsSuccess.Should().BeTrue();
        var data = result.Data;
        data.Should().NotBeNull();
        data!.Count.Should().Be(0);
    }

    // ============================================================
    // CreateAsync
    // ============================================================

    [Fact]
    public async Task CreateAsync_ShouldCreateFactory()
    {
        var command = new FactoryCreateCommand
        {
            Code = "F001",
            Name = "新工厂",
            Address = "上海市某区某路",
        };

        var result = await FactoryService.CreateAsync(command);

        result.IsSuccess.Should().BeTrue();
        var factories = await AllFactoriesAsync();
        factories.Should().ContainSingle();
        factories[0].Code.Should().Be("F001");
    }

    [Fact]
    public async Task CreateAsync_DuplicateCode_ShouldThrow()
    {
        Db.Seed(new FactoryEntity { Code = "F001", Name = "已有工厂" });

        var command = new FactoryCreateCommand { Code = "F001", Name = "新工厂" };

        var act = () => FactoryService.CreateAsync(command);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("已存在"));
    }

    // ============================================================
    // UpdateAsync
    // ============================================================

    [Fact]
    public async Task UpdateAsync_ShouldUpdateFactory()
    {
        var factory = new FactoryEntity { Code = "F001", Name = "原名称" };
        Db.Seed(factory);

        var command = new FactoryUpdateCommand
        {
            Id = factory.Id,
            Code = "F001",
            Name = "新名称",
            Status = EnableStatusEnum.Disabled,
        };

        var result = await FactoryService.UpdateAsync(command);

        result.IsSuccess.Should().BeTrue();
        var updated = await Client.Queryable<FactoryEntity>().FirstAsync(f => f.Id == factory.Id);
        updated.Name.Should().Be("新名称");
        updated.Status.Should().Be(EnableStatusEnum.Disabled);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ShouldThrow()
    {
        var command = new FactoryUpdateCommand
        {
            Id = 99999,
            Code = "F999",
            Name = "不存在的工厂",
        };

        var act = () => FactoryService.UpdateAsync(command);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("不存在"));
    }

    // ============================================================
    // DeleteAsync
    // ============================================================

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete()
    {
        var factory = new FactoryEntity { Code = "F001", Name = "待删除工厂" };
        Db.Seed(factory);

        var result = await FactoryService.DeleteAsync(factory.Id);

        result.IsSuccess.Should().BeTrue();
        // Soft delete: entity should not be queryable normally
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ShouldThrow()
    {
        var act = () => FactoryService.DeleteAsync(99999);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("不存在"));
    }

    // ============================================================
    // BatchDeleteAsync
    // ============================================================

    [Fact]
    public async Task BatchDeleteAsync_ShouldBatchSoftDelete()
    {
        var f1 = new FactoryEntity { Code = "F001", Name = "工厂一" };
        var f2 = new FactoryEntity { Code = "F002", Name = "工厂二" };
        Db.Seed(f1, f2);

        var result = await FactoryService.BatchDeleteAsync(new List<long> { f1.Id, f2.Id });

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task BatchDeleteAsync_EmptyList_ShouldThrow()
    {
        var act = () => FactoryService.BatchDeleteAsync(new List<long>());

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("选择"));
    }

    // ============================================================
    // Additional Field Tests
    // ============================================================

    [Fact]
    public async Task CreateAsync_ShouldPersistAddress()
    {
        var command = new FactoryCreateCommand
        {
            Code = "F001",
            Name = "工厂一",
            Address = "北京市朝阳区建国路88号",
        };

        var result = await FactoryService.CreateAsync(command);

        result.IsSuccess.Should().BeTrue();
        var factories = await AllFactoriesAsync();
        factories[0].Address.Should().Be("北京市朝阳区建国路88号");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateAddress()
    {
        var factory = new FactoryEntity { Code = "F001", Name = "工厂一", Address = "原地址" };
        Db.Seed(factory);

        var command = new FactoryUpdateCommand
        {
            Id = factory.Id,
            Code = "F001",
            Name = "工厂一",
            Address = "新地址上海市浦东新区",
        };

        var result = await FactoryService.UpdateAsync(command);

        result.IsSuccess.Should().BeTrue();
        var updated = await Client.Queryable<FactoryEntity>().FirstAsync(f => f.Id == factory.Id);
        updated.Address.Should().Be("新地址上海市浦东新区");
    }

    [Fact]
    public async Task PageAsync_ShouldFilterByName()
    {
        Db.Seed(
            new FactoryEntity { Code = "F001", Name = "北京工厂" },
            new FactoryEntity { Code = "F002", Name = "上海工厂" }
        );

        var query = new FactoryPageQuery { Name = "北京", Page = 1, PageSize = 10 };

        var result = await FactoryService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        var data = result.Data;
        data!.Total.Should().Be(1);
    }

    [Fact]
    public async Task PageAsync_ShouldFilterByStatus()
    {
        Db.Seed(
            new FactoryEntity { Code = "F001", Name = "启用工厂", Status = EnableStatusEnum.Enabled },
            new FactoryEntity { Code = "F002", Name = "禁用工厂", Status = EnableStatusEnum.Disabled }
        );

        var query = new FactoryPageQuery { Status = (int)EnableStatusEnum.Enabled, Page = 1, PageSize = 10 };

        var result = await FactoryService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        var data = result.Data;
        data!.Total.Should().Be(1);
    }

    [Fact]
    public async Task PageAsync_ShouldPaginate()
    {
        for (int i = 1; i <= 15; i++)
        {
            Db.Seed(new FactoryEntity { Code = $"F{i:D3}", Name = $"工厂{i}" });
        }

        var query = new FactoryPageQuery { Page = 2, PageSize = 5 };

        var result = await FactoryService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        var data = result.Data;
        data!.Total.Should().Be(15);
        data.Items.Count.Should().Be(5);
    }
}
