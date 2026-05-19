using FluentAssertions;
using Lzq.QA.Application.Contracts.Commands;
using Lzq.QA.Application.Contracts.Queries;
using Lzq.QA.Domain.Entities;
using Lzq.QA.Domain.Enums;
using Lzq.QA.Tests.Helpers;
using Xunit;

namespace Lzq.QA.Tests.Services;

[Collection("QA Tests")]
public class DefectRecordServiceTests : ServiceTestBase
{
    // ============================================================
    // PageAsync
    // ============================================================

    [Fact]
    public async Task PageAsync_ShouldReturnPagedDefects()
    {
        Db.Seed(
            new DefectRecordEntity { QCOrderCode = "QC001", WorkOrderCode = "WO001", ProductName = "产品A", BatchNo = "B001",
                DefectCode = "D001", Status = DefectStatusEnum.Pending },
            new DefectRecordEntity { QCOrderCode = "QC002", WorkOrderCode = "WO002", ProductName = "产品B", BatchNo = "B002",
                DefectCode = "D002", Status = DefectStatusEnum.Processed }
        );

        var query = new DefectRecordPageQuery { Page = 1, PageSize = 10 };

        var result = await DefectService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Total.Should().Be(2);
    }

    [Fact]
    public async Task PageAsync_ShouldFilterByStatus()
    {
        Db.Seed(
            new DefectRecordEntity { QCOrderCode = "QC001", WorkOrderCode = "WO001", ProductName = "产品A",
                DefectCode = "D001", Status = DefectStatusEnum.Pending },
            new DefectRecordEntity { QCOrderCode = "QC002", WorkOrderCode = "WO002", ProductName = "产品B",
                DefectCode = "D002", Status = DefectStatusEnum.Processed }
        );

        var query = new DefectRecordPageQuery { Status = DefectStatusEnum.Pending, Page = 1, PageSize = 10 };

        var result = await DefectService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Total.Should().Be(1);
    }

    [Fact]
    public async Task PageAsync_ShouldPaginate()
    {
        for (int i = 1; i <= 15; i++)
        {
            Db.Seed(new DefectRecordEntity { QCOrderCode = $"QC{i:D3}", WorkOrderCode = $"WO{i:D3}",
                ProductName = $"产品{i}", DefectCode = $"D{i:D3}", Status = DefectStatusEnum.Pending });
        }

        var query = new DefectRecordPageQuery { Page = 2, PageSize = 5 };

        var result = await DefectService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        var data = result.Data;
        data!.Total.Should().Be(15);
        data.Items.Count.Should().Be(5);
    }

    [Fact]
    public async Task PageAsync_ShouldFilterByDefectCode()
    {
        Db.Seed(
            new DefectRecordEntity { QCOrderCode = "QC001", WorkOrderCode = "WO001", ProductName = "产品A",
                DefectCode = "D001", Status = DefectStatusEnum.Pending },
            new DefectRecordEntity { QCOrderCode = "QC002", WorkOrderCode = "WO002", ProductName = "产品B",
                DefectCode = "D002", Status = DefectStatusEnum.Pending }
        );

        var query = new DefectRecordPageQuery { DefectCode = "D001", Page = 1, PageSize = 10 };

        var result = await DefectService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Total.Should().Be(1);
    }

    // ============================================================
    // GetAsync
    // ============================================================

    [Fact]
    public async Task GetAsync_ShouldReturnDefect()
    {
        var defect = new DefectRecordEntity { QCOrderCode = "QC001", WorkOrderCode = "WO001",
            ProductName = "产品A", DefectCode = "D001", Status = DefectStatusEnum.Pending };
        Db.Seed(defect);

        var result = await DefectService.GetAsync(defect.Id);

        result.IsSuccess.Should().BeTrue();
        result.Data!.QCOrderCode.Should().Be("QC001");
    }

    [Fact]
    public async Task GetAsync_NotFound_ShouldThrow()
    {
        var act = () => DefectService.GetAsync(99999);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("不存在"));
    }

    // ============================================================
    // CreateAsync
    // ============================================================

    [Fact]
    public async Task CreateAsync_ShouldCreateDefect()
    {
        var command = new DefectRecordCreateCommand
        {
            QCOrderCode = "QC001",
            WorkOrderCode = "WO001",
            ProductName = "产品A",
            DefectCode = "D001",
            DefectDesc = "划痕",
            DefectQty = 5,
        };

        var result = await DefectService.CreateAsync(command);

        result.IsSuccess.Should().BeTrue();
        var defects = await AllDefectsAsync();
        defects.Should().ContainSingle();
        defects[0].DefectCode.Should().Be("D001");
        defects[0].Status.Should().Be(DefectStatusEnum.Pending);
    }

    // ============================================================
    // DeleteAsync
    // ============================================================

    [Fact]
    public async Task DeleteAsync_ShouldDeletePendingDefect()
    {
        var defect = new DefectRecordEntity { QCOrderCode = "QC001", WorkOrderCode = "WO001",
            ProductName = "产品A", DefectCode = "D001", Status = DefectStatusEnum.Pending };
        Db.Seed(defect);

        var result = await DefectService.DeleteAsync(defect.Id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ProcessedDefect_ShouldThrow()
    {
        var defect = new DefectRecordEntity { QCOrderCode = "QC001", WorkOrderCode = "WO001",
            ProductName = "产品A", DefectCode = "D001", Status = DefectStatusEnum.Processed };
        Db.Seed(defect);

        var act = () => DefectService.DeleteAsync(defect.Id);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("待处理"));
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ShouldThrow()
    {
        var act = () => DefectService.DeleteAsync(99999);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("不存在"));
    }

    // ============================================================
    // HandleAsync
    // ============================================================

    [Fact]
    public async Task HandleAsync_ShouldHandlePendingDefect()
    {
        var defect = new DefectRecordEntity { QCOrderCode = "QC001", WorkOrderCode = "WO001",
            ProductName = "产品A", DefectCode = "D001", Status = DefectStatusEnum.Pending };
        Db.Seed(defect);

        var command = new DefectRecordHandleCommand
        {
            Id = defect.Id,
            HandlingType = DefectHandlingEnum.Rework,
            HandlingRemark = "返工处理",
        };

        var result = await DefectService.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        var updated = await Client.Queryable<DefectRecordEntity>().FirstAsync(d => d.Id == defect.Id);
        updated.Status.Should().Be(DefectStatusEnum.Processed);
        updated.HandlingType.Should().Be(DefectHandlingEnum.Rework);
    }

    [Fact]
    public async Task HandleAsync_AlreadyProcessed_ShouldThrow()
    {
        var defect = new DefectRecordEntity { QCOrderCode = "QC001", WorkOrderCode = "WO001",
            ProductName = "产品A", DefectCode = "D001", Status = DefectStatusEnum.Processed };
        Db.Seed(defect);

        var command = new DefectRecordHandleCommand { Id = defect.Id, HandlingType = DefectHandlingEnum.Scrap };

        var act = () => DefectService.HandleAsync(command);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("已处理"));
    }

    // ============================================================
    // ReviewAsync
    // ============================================================

    [Fact]
    public async Task ReviewAsync_ShouldReviewDefect()
    {
        var defect = new DefectRecordEntity { QCOrderCode = "QC001", WorkOrderCode = "WO001",
            ProductName = "产品A", DefectCode = "D001", Status = DefectStatusEnum.Pending, NeedReview = true };
        Db.Seed(defect);

        var command = new DefectRecordReviewCommand
        {
            Id = defect.Id,
            ReviewResult = "同意返工",
            HandlingType = DefectHandlingEnum.Rework,
        };

        var result = await DefectService.ReviewAsync(command);

        result.IsSuccess.Should().BeTrue();
        var updated = await Client.Queryable<DefectRecordEntity>().FirstAsync(d => d.Id == defect.Id);
        updated.Status.Should().Be(DefectStatusEnum.Processed);
        updated.HandlingType.Should().Be(DefectHandlingEnum.Rework);
        updated.ReviewResult.Should().Be("同意返工");
    }

    [Fact]
    public async Task ReviewAsync_NotNeedReview_ShouldThrow()
    {
        var defect = new DefectRecordEntity { QCOrderCode = "QC001", WorkOrderCode = "WO001",
            ProductName = "产品A", DefectCode = "D001", Status = DefectStatusEnum.Pending, NeedReview = false };
        Db.Seed(defect);

        var command = new DefectRecordReviewCommand { Id = defect.Id, ReviewResult = "通过" };

        var act = () => DefectService.ReviewAsync(command);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("评审"));
    }
}
