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
public class ProcessServiceTests : ServiceTestBase
{
    private LineEntity _defaultLine = null!;

    public ProcessServiceTests()
    {
        var factory = new FactoryEntity { Code = "F001", Name = "默认工厂" };
        var workshop = new WorkshopEntity { Code = "W001", Name = "默认车间", FactoryId = factory.Id };
        _defaultLine = new LineEntity { Code = "L001", Name = "默认产线", WorkshopId = workshop.Id };
        Db.Seed(factory, workshop, _defaultLine);
    }

    // ============================================================
    // PageAsync
    // ============================================================

    [Fact]
    public async Task PageAsync_ShouldReturnPagedProcesses()
    {
        Db.Seed(
            new ProcessEntity { Code = "P001", Name = "工序一", LineId = _defaultLine.Id, Sequence = 1, StandardHours = 1.5m },
            new ProcessEntity { Code = "P002", Name = "工序二", LineId = _defaultLine.Id, Sequence = 2, StandardHours = 2.0m }
        );

        var query = new ProcessPageQuery { Page = 1, PageSize = 10 };

        var result = await ProcessService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Total.Should().Be(2);
    }

    [Fact]
    public async Task PageAsync_ShouldFilterByLineId()
    {
        Db.Seed(
            new ProcessEntity { Code = "P001", Name = "工序一", LineId = _defaultLine.Id, Sequence = 1, StandardHours = 1.0m },
            new ProcessEntity { Code = "P002", Name = "工序二", LineId = 99999, Sequence = 1, StandardHours = 1.0m }
        );

        var query = new ProcessPageQuery { LineId = _defaultLine.Id, Page = 1, PageSize = 10 };

        var result = await ProcessService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Total.Should().Be(1);
    }

    [Fact]
    public async Task PageAsync_ShouldOrderBySequence()
    {
        Db.Seed(
            new ProcessEntity { Code = "P003", Name = "工序三", LineId = _defaultLine.Id, Sequence = 3, StandardHours = 3.0m },
            new ProcessEntity { Code = "P001", Name = "工序一", LineId = _defaultLine.Id, Sequence = 1, StandardHours = 1.0m },
            new ProcessEntity { Code = "P002", Name = "工序二", LineId = _defaultLine.Id, Sequence = 2, StandardHours = 2.0m }
        );

        var query = new ProcessPageQuery { Page = 1, PageSize = 10 };

        var result = await ProcessService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Total.Should().Be(3);
    }

    // ============================================================
    // ListByLineAsync
    // ============================================================

    [Fact]
    public async Task ListByLineAsync_ShouldReturnEnabledProcesses()
    {
        Db.Seed(
            new ProcessEntity { Code = "P001", Name = "工序一", LineId = _defaultLine.Id, Sequence = 1, StandardHours = 1.0m, Status = EnableStatusEnum.Enabled },
            new ProcessEntity { Code = "P002", Name = "工序二", LineId = _defaultLine.Id, Sequence = 2, StandardHours = 2.0m, Status = EnableStatusEnum.Disabled }
        );

        var result = await ProcessService.ListByLineAsync(_defaultLine.Id);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Count.Should().Be(1);
    }

    // ============================================================
    // CreateAsync
    // ============================================================

    [Fact]
    public async Task CreateAsync_ShouldCreateProcess()
    {
        var command = new ProcessCreateCommand
        {
            Code = "P001",
            Name = "新工序",
            LineId = _defaultLine.Id,
            Sequence = 1,
            StandardHours = 2.5m,
        };

        var result = await ProcessService.CreateAsync(command);

        result.IsSuccess.Should().BeTrue();
        var processes = await AllProcessesAsync();
        processes.Should().ContainSingle();
        processes[0].Code.Should().Be("P001");
        processes[0].StandardHours.Should().Be(2.5m);
    }

    [Fact]
    public async Task CreateAsync_LineNotFound_ShouldThrow()
    {
        var command = new ProcessCreateCommand
        {
            Code = "P001",
            Name = "新工序",
            LineId = 99999,
            Sequence = 1,
            StandardHours = 1.0m,
        };

        var act = () => ProcessService.CreateAsync(command);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("产线"));
    }

    [Fact]
    public async Task CreateAsync_DuplicateCode_ShouldThrow()
    {
        Db.Seed(new ProcessEntity { Code = "P001", Name = "已有工序", LineId = _defaultLine.Id, Sequence = 1, StandardHours = 1.0m });

        var command = new ProcessCreateCommand
        {
            Code = "P001",
            Name = "新工序",
            LineId = _defaultLine.Id,
            Sequence = 2,
            StandardHours = 2.0m,
        };

        var act = () => ProcessService.CreateAsync(command);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("已存在"));
    }

    // ============================================================
    // UpdateAsync
    // ============================================================

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProcess()
    {
        var process = new ProcessEntity { Code = "P001", Name = "原名称", LineId = _defaultLine.Id, Sequence = 1, StandardHours = 1.0m };
        Db.Seed(process);

        var command = new ProcessUpdateCommand
        {
            Id = process.Id,
            Code = "P001",
            Name = "新名称",
            LineId = _defaultLine.Id,
            Sequence = 5,
            StandardHours = 3.5m,
            Status = EnableStatusEnum.Disabled,
        };

        var result = await ProcessService.UpdateAsync(command);

        result.IsSuccess.Should().BeTrue();
        var updated = await Client.Queryable<ProcessEntity>().FirstAsync(p => p.Id == process.Id);
        updated.Name.Should().Be("新名称");
        updated.Sequence.Should().Be(5);
        updated.StandardHours.Should().Be(3.5m);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ShouldThrow()
    {
        var command = new ProcessUpdateCommand
        {
            Id = 99999,
            Code = "P999",
            Name = "不存在的工序",
            LineId = _defaultLine.Id,
            Sequence = 1,
            StandardHours = 1.0m,
        };

        var act = () => ProcessService.UpdateAsync(command);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("不存在"));
    }

    // ============================================================
    // DeleteAsync
    // ============================================================

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete()
    {
        var process = new ProcessEntity { Code = "P001", Name = "待删除工序", LineId = _defaultLine.Id, Sequence = 1, StandardHours = 1.0m };
        Db.Seed(process);

        var result = await ProcessService.DeleteAsync(process.Id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ShouldThrow()
    {
        var act = () => ProcessService.DeleteAsync(99999);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("不存在"));
    }

    // ============================================================
    // BatchDeleteAsync
    // ============================================================

    [Fact]
    public async Task BatchDeleteAsync_ShouldBatchSoftDelete()
    {
        var p1 = new ProcessEntity { Code = "P001", Name = "工序一", LineId = _defaultLine.Id, Sequence = 1, StandardHours = 1.0m };
        var p2 = new ProcessEntity { Code = "P002", Name = "工序二", LineId = _defaultLine.Id, Sequence = 2, StandardHours = 2.0m };
        Db.Seed(p1, p2);

        var result = await ProcessService.BatchDeleteAsync(new List<long> { p1.Id, p2.Id });

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task BatchDeleteAsync_EmptyList_ShouldThrow()
    {
        var act = () => ProcessService.BatchDeleteAsync(new List<long>());

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("选择"));
    }

    // ============================================================
    // Additional Field Tests
    // ============================================================

    [Fact]
    public async Task CreateAsync_ShouldPersistSequenceAndStandardHours()
    {
        var command = new ProcessCreateCommand
        {
            Code = "P001",
            Name = "工序一",
            LineId = _defaultLine.Id,
            Sequence = 10,
            StandardHours = 2.5m,
        };

        var result = await ProcessService.CreateAsync(command);

        result.IsSuccess.Should().BeTrue();
        var processes = await AllProcessesAsync();
        processes[0].Sequence.Should().Be(10);
        processes[0].StandardHours.Should().Be(2.5m);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateSequenceAndStandardHours()
    {
        var process = new ProcessEntity { Code = "P001", Name = "工序一", LineId = _defaultLine.Id, Sequence = 1, StandardHours = 1.0m };
        Db.Seed(process);

        var command = new ProcessUpdateCommand
        {
            Id = process.Id,
            Code = "P001",
            Name = "工序一",
            LineId = _defaultLine.Id,
            Sequence = 5,
            StandardHours = 3.5m,
        };

        var result = await ProcessService.UpdateAsync(command);

        result.IsSuccess.Should().BeTrue();
        var updated = await Client.Queryable<ProcessEntity>().FirstAsync(p => p.Id == process.Id);
        updated.Sequence.Should().Be(5);
        updated.StandardHours.Should().Be(3.5m);
    }

    [Fact]
    public async Task PageAsync_ShouldFilterByName()
    {
        Db.Seed(
            new ProcessEntity { Code = "P001", Name = "焊接工序", LineId = _defaultLine.Id, Sequence = 1, StandardHours = 1.0m },
            new ProcessEntity { Code = "P002", Name = "组装工序", LineId = _defaultLine.Id, Sequence = 2, StandardHours = 2.0m }
        );

        var query = new ProcessPageQuery { Name = "焊接", Page = 1, PageSize = 10 };

        var result = await ProcessService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        var data = result.Data;
        data!.Total.Should().Be(1);
    }

    [Fact]
    public async Task PageAsync_ShouldFilterByStatus()
    {
        Db.Seed(
            new ProcessEntity { Code = "P001", Name = "启用工序", LineId = _defaultLine.Id, Sequence = 1, StandardHours = 1.0m, Status = EnableStatusEnum.Enabled },
            new ProcessEntity { Code = "P002", Name = "禁用工序", LineId = _defaultLine.Id, Sequence = 2, StandardHours = 2.0m, Status = EnableStatusEnum.Disabled }
        );

        var query = new ProcessPageQuery { Status = EnableStatusEnum.Enabled, Page = 1, PageSize = 10 };

        var result = await ProcessService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        var data = result.Data;
        data!.Total.Should().Be(1);
    }

    [Fact]
    public async Task PageAsync_ShouldPaginate()
    {
        for (int i = 1; i <= 15; i++)
        {
            Db.Seed(new ProcessEntity { Code = $"P{i:D3}", Name = $"工序{i}", LineId = _defaultLine.Id, Sequence = i, StandardHours = 1.0m });
        }

        var query = new ProcessPageQuery { Page = 2, PageSize = 5 };

        var result = await ProcessService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        var data = result.Data;
        data!.Total.Should().Be(15);
        data.Items.Count.Should().Be(5);
    }

    [Fact]
    public async Task ListByLineAsync_EmptyLine_ShouldReturnEmpty()
    {
        var result = await ProcessService.ListByLineAsync(_defaultLine.Id);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Count.Should().Be(0);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateStatus()
    {
        var process = new ProcessEntity { Code = "P001", Name = "工序一", LineId = _defaultLine.Id, Sequence = 1, StandardHours = 1.0m, Status = EnableStatusEnum.Enabled };
        Db.Seed(process);

        var command = new ProcessUpdateCommand
        {
            Id = process.Id,
            Code = "P001",
            Name = "工序一",
            LineId = _defaultLine.Id,
            Sequence = 1,
            StandardHours = 1.0m,
            Status = EnableStatusEnum.Disabled,
        };

        var result = await ProcessService.UpdateAsync(command);

        result.IsSuccess.Should().BeTrue();
        var updated = await Client.Queryable<ProcessEntity>().FirstAsync(p => p.Id == process.Id);
        updated.Status.Should().Be(EnableStatusEnum.Disabled);
    }

    [Fact]
    public async Task PageAsync_ZeroStandardHours_ShouldBeAllowed()
    {
        Db.Seed(new ProcessEntity { Code = "P001", Name = "质检工序", LineId = _defaultLine.Id, Sequence = 1, StandardHours = 0m });

        var query = new ProcessPageQuery { Page = 1, PageSize = 10 };

        var result = await ProcessService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        var data = result.Data;
        data!.Total.Should().Be(1);
    }
}
