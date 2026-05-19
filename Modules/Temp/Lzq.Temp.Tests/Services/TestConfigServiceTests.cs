using FluentAssertions;
using Lzq.Temp.Application.Contracts.TestConfig.Commands;
using Lzq.Temp.Application.Contracts.TestConfig.Dto;
using Lzq.Temp.Application.Contracts.TestConfig.Queries;
using Lzq.Temp.Domain.Entities.TestConfig;
using Lzq.Temp.Tests.Helpers;
using Xunit;

namespace Lzq.Temp.Tests.Services;

[Collection("Temp Tests")]
public class TestConfigServiceTests : ServiceTestBase
{
    // ============================================================
    // PageAsync
    // ============================================================

    [Fact]
    public async Task PageAsync_ShouldReturnPagedConfigs()
    {
        Db.Seed(
            new TestConfigEntity { Code = "C001", Name = "配置1", ConfigType = 1 },
            new TestConfigEntity { Code = "C002", Name = "配置2", ConfigType = 2 }
        );

        var query = new TestConfigPageQuery { Page = 1, PageSize = 10 };

        var result = await TestConfigService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Total.Should().Be(2);
    }

    [Fact]
    public async Task PageAsync_ShouldFilterByKeyword()
    {
        Db.Seed(
            new TestConfigEntity { Code = "C001", Name = "系统配置", ConfigType = 1 },
            new TestConfigEntity { Code = "C002", Name = "业务配置", ConfigType = 2 }
        );

        var query = new TestConfigPageQuery { Keyword = "系统", Page = 1, PageSize = 10 };

        var result = await TestConfigService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Total.Should().Be(1);
    }

    [Fact]
    public async Task PageAsync_ShouldPaginate()
    {
        for (int i = 1; i <= 15; i++)
        {
            Db.Seed(new TestConfigEntity { Code = $"C{i:D3}", Name = $"配置{i}", ConfigType = 1 });
        }

        var query = new TestConfigPageQuery { Page = 2, PageSize = 5 };

        var result = await TestConfigService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Total.Should().Be(15);
        result.Data.Items.Count.Should().Be(5);
    }

    // ============================================================
    // GetAsync
    // ============================================================

    [Fact]
    public async Task GetAsync_ShouldReturnConfig()
    {
        var config = new TestConfigEntity { Code = "C001", Name = "测试配置", ConfigType = 1 };
        Db.Seed(config);

        var result = await TestConfigService.GetAsync(config.Id);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Code.Should().Be("C001");
        result.Data.Name.Should().Be("测试配置");
    }

    [Fact]
    public async Task GetAsync_NotFound_ShouldThrow()
    {
        var act = () => TestConfigService.GetAsync(99999);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("不存在"));
    }

    // ============================================================
    // CreateAsync
    // ============================================================

    [Fact]
    public async Task CreateAsync_ShouldCreateConfig()
    {
        var command = new TestConfigCreateCommand
        {
            Code = "C001",
            Name = "新配置",
            ConfigType = 1,
            IsEnabled = true,
        };

        var result = await TestConfigService.CreateAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeGreaterThan(0);

        var configs = await AllConfigsAsync();
        configs.Should().ContainSingle();
        configs[0].Code.Should().Be("C001");
        configs[0].Name.Should().Be("新配置");
    }

    // ============================================================
    // UpdateAsync
    // ============================================================

    [Fact]
    public async Task UpdateAsync_ShouldUpdateConfig()
    {
        var config = new TestConfigEntity { Code = "C001", Name = "原名称", ConfigType = 1 };
        Db.Seed(config);

        var command = new TestConfigUpdateCommand
        {
            Id = config.Id,
            Code = "C001",
            Name = "新名称",
            ConfigType = 2,
            IsEnabled = false,
        };

        var result = await TestConfigService.UpdateAsync(command);

        result.IsSuccess.Should().BeTrue();

        var updated = await AllConfigsAsync();
        updated.Should().ContainSingle();
        updated[0].Name.Should().Be("新名称");
        updated[0].ConfigType.Should().Be(2);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ShouldThrow()
    {
        var command = new TestConfigUpdateCommand
        {
            Id = 99999,
            Code = "C001",
            Name = "不存在",
            ConfigType = 1,
        };

        var act = () => TestConfigService.UpdateAsync(command);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("不存在"));
    }

    // ============================================================
    // DeleteAsync
    // ============================================================

    [Fact]
    public async Task DeleteAsync_ShouldDeleteConfig()
    {
        var config = new TestConfigEntity { Code = "C001", Name = "待删除", ConfigType = 1 };
        Db.Seed(config);

        var result = await TestConfigService.DeleteAsync(config.Id);

        result.IsSuccess.Should().BeTrue();

        var configs = await AllConfigsAsync();
        configs.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ShouldThrow()
    {
        var act = () => TestConfigService.DeleteAsync(99999);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("不存在"));
    }
}
