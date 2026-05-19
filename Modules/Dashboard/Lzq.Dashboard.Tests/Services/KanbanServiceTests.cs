using FluentAssertions;
using Lzq.Dashboard.Application.Contracts.Dtos;
using Lzq.Dashboard.Domain.Entities;
using Lzq.Dashboard.Tests.Helpers;
using Xunit;

namespace Lzq.Dashboard.Tests.Services;

[Collection("Dashboard Tests")]
public class KanbanServiceTests : ServiceTestBase
{
    // ============================================================
    // GetConfigListAsync
    // ============================================================

    [Fact]
    public async Task GetConfigListAsync_ShouldReturnAllConfigs()
    {
        Db.Seed(
            new DashboardConfigEntity { Code = "C001", Name = "配置一", ConfigType = 1 },
            new DashboardConfigEntity { Code = "C002", Name = "配置二", ConfigType = 1 },
            new DashboardConfigEntity { Code = "C003", Name = "配置三", ConfigType = 2 }
        );

        var result = await KanbanService.GetConfigListAsync();

        result.Data.Should().NotBeNull();
        result.Data.Count.Should().Be(3);
    }

    [Fact]
    public async Task GetConfigListAsync_ShouldFilterByConfigType()
    {
        Db.Seed(
            new DashboardConfigEntity { Code = "C001", Name = "配置一", ConfigType = 1 },
            new DashboardConfigEntity { Code = "C002", Name = "配置二", ConfigType = 2 }
        );

        var result = await KanbanService.GetConfigListAsync(configType: 1);

        result.Should().NotBeNull();
        result.Data.Count.Should().Be(1);
        result.Data[0].Code.Should().Be("C001");
    }

    [Fact]
    public async Task GetConfigListAsync_EmptyTable_ShouldReturnEmptyList()
    {
        var result = await KanbanService.GetConfigListAsync();

        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();
    }

    // ============================================================
    // GetConfigAsync
    // ============================================================

    [Fact]
    public async Task GetConfigAsync_ShouldReturnConfigById()
    {
        var config = new DashboardConfigEntity { Code = "C001", Name = "测试配置" };
        Db.Seed(config);

        var result = await KanbanService.GetConfigAsync(config.Id);

        result.Data.Should().NotBeNull();
        result.Data!.Code.Should().Be("C001");
        result.Data.Name.Should().Be("测试配置");
    }

    [Fact]
    public async Task GetConfigAsync_NotFound_ShouldReturnNull()
    {
        var result = await KanbanService.GetConfigAsync(99999);

        result.Should().BeNull();
    }

    // ============================================================
    // CreateConfigAsync
    // ============================================================

    [Fact]
    public async Task CreateConfigAsync_ShouldCreateConfig()
    {
        var dto = new DashboardConfigDto
        {
            Code = "C001",
            Name = "新配置",
            ConfigType = 1,
            RefreshInterval = 30,
            CacheTtl = 60,
            IsEnabled = true,
        };

        var result = await KanbanService.CreateConfigAsync(dto);

        result.Data.Should().NotBeNull();
        result.Data.Code.Should().Be("C001");
        var configs = await AllConfigsAsync();
        configs.Should().ContainSingle();
        configs[0].Name.Should().Be("新配置");
        configs[0].RefreshInterval.Should().Be(30);
    }

    [Fact]
    public async Task CreateConfigAsync_ShouldSetDefaultValues()
    {
        var dto = new DashboardConfigDto
        {
            Code = "C001",
            Name = "默认值测试",
        };

        var result = await KanbanService.CreateConfigAsync(dto);

        result.Data.RefreshInterval.Should().Be(0); // default int
    }

    // ============================================================
    // UpdateConfigAsync
    // ============================================================

    [Fact]
    public async Task UpdateConfigAsync_ShouldUpdateConfig()
    {
        var config = new DashboardConfigEntity { Code = "C001", Name = "原名称", ConfigType = 1 };
        Db.Seed(config);

        var dto = new DashboardConfigDto
        {
            Id = config.Id,
            Code = "C001",
            Name = "新名称",
            ConfigType = 1,
            RefreshInterval = 120,
            CacheTtl = 600,
            IsEnabled = false,
            Remark = "测试备注",
        };

        var result = await KanbanService.UpdateConfigAsync(dto);

        result.Data.Should().NotBeNull();
        result.Data.Name.Should().Be("新名称");
        var updated = await Client.Queryable<DashboardConfigEntity>().FirstAsync(c => c.Id == config.Id);
        updated.Name.Should().Be("新名称");
        updated.RefreshInterval.Should().Be(120);
        updated.IsEnabled.Should().BeFalse();
        updated.Remark.Should().Be("测试备注");
    }

    [Fact]
    public async Task UpdateConfigAsync_NotFound_ShouldThrow()
    {
        var dto = new DashboardConfigDto
        {
            Id = 99999,
            Code = "C999",
            Name = "不存在的配置",
        };

        var act = () => KanbanService.UpdateConfigAsync(dto);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("不存在"));
    }

    // ============================================================
    // DeleteConfigAsync
    // ============================================================

    [Fact]
    public async Task DeleteConfigAsync_ShouldDeleteConfig()
    {
        var config = new DashboardConfigEntity { Code = "C001", Name = "待删除配置" };
        Db.Seed(config);

        await KanbanService.DeleteConfigAsync(config.Id);

        // 验证已删除
        var configs = await AllConfigsAsync();
        configs.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteConfigAsync_NotFound_ShouldNotThrow()
    {
        // 删除不存在的实体不应抛异常（ISqlSugarRepository.DeleteByIdAsync 行为）
        await KanbanService.DeleteConfigAsync(99999);

        // 不应抛出异常即为通过
    }
}
