using FluentAssertions;
using Lzq.MES.Application.Contracts.Commands;
using Lzq.MES.Application.Contracts.Queries;
using Lzq.MES.Domain.Entities;
using Lzq.MES.Domain.Enums;
using Lzq.MES.Tests.Helpers;
using Xunit;

namespace Lzq.MES.Tests.Services;

[Collection("QA Tests")]
public class QCOrderServiceTests : ServiceTestBase
{
    // ============================================================
    // PageAsync
    // ============================================================

    [Fact]
    public async Task PageAsync_ShouldReturnPagedQCOrders()
    {
        Db.Seed(
            new QCOrderEntity { Code = "QC001", QCType = QCTypeEnum.PQC, ProductName = "产品A",
                Status = QCOrderStatusEnum.Pending },
            new QCOrderEntity { Code = "QC002", QCType = QCTypeEnum.PQC, ProductName = "产品B",
                Status = QCOrderStatusEnum.Qualified }
        );

        var query = new QCOrderPageQuery { Page = 1, PageSize = 10 };

        var result = await QCOrderService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Total.Should().Be(2);
    }

    [Fact]
    public async Task PageAsync_ShouldFilterByStatus()
    {
        Db.Seed(
            new QCOrderEntity { Code = "QC001", QCType = QCTypeEnum.PQC, ProductName = "产品A",
                Status = QCOrderStatusEnum.Pending },
            new QCOrderEntity { Code = "QC002", QCType = QCTypeEnum.PQC, ProductName = "产品B",
                Status = QCOrderStatusEnum.Qualified }
        );

        var query = new QCOrderPageQuery { Status = QCOrderStatusEnum.Pending, Page = 1, PageSize = 10 };

        var result = await QCOrderService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Total.Should().Be(1);
    }

    [Fact]
    public async Task PageAsync_ShouldPaginate()
    {
        for (int i = 1; i <= 15; i++)
        {
            Db.Seed(new QCOrderEntity { Code = $"QC{i:D3}", QCType = QCTypeEnum.PQC, ProductName = $"产品{i}", Status = QCOrderStatusEnum.Pending });
        }

        var query = new QCOrderPageQuery { Page = 2, PageSize = 5 };

        var result = await QCOrderService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        var data = result.Data;
        data!.Total.Should().Be(15);
        data.Items.Count.Should().Be(5);
    }

    [Fact]
    public async Task PageAsync_ShouldFilterByCode()
    {
        Db.Seed(
            new QCOrderEntity { Code = "QC001", QCType = QCTypeEnum.PQC, ProductName = "产品A", Status = QCOrderStatusEnum.Pending },
            new QCOrderEntity { Code = "QC002", QCType = QCTypeEnum.PQC, ProductName = "产品B", Status = QCOrderStatusEnum.Pending }
        );

        var query = new QCOrderPageQuery { Code = "001", Page = 1, PageSize = 10 };

        var result = await QCOrderService.PageAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Total.Should().Be(1);
    }

    // ============================================================
    // GetAsync
    // ============================================================

    [Fact]
    public async Task GetAsync_ShouldReturnQCOrder()
    {
        var order = new QCOrderEntity { Code = "QC001", QCType = QCTypeEnum.PQC,
            ProductName = "产品A", Status = QCOrderStatusEnum.Pending };
        Db.Seed(order);

        var result = await QCOrderService.GetAsync(order.Id);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Code.Should().Be("QC001");
    }

    [Fact]
    public async Task GetAsync_NotFound_ShouldThrow()
    {
        var act = () => QCOrderService.GetAsync(99999);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("不存在"));
    }

    // ============================================================
    // CreateAsync
    // ============================================================

    [Fact]
    public async Task CreateAsync_ShouldCreatePQCQCOrder()
    {
        var command = new QCOrderCreateCommand
        {
            QCType = QCTypeEnum.PQC,
            ProductName = "产品A",
            SubmitQty = 100,
        };

        var result = await QCOrderService.CreateAsync(command);

        result.IsSuccess.Should().BeTrue();
        var orders = await AllQCOrdersAsync();
        orders.Should().ContainSingle();
        orders[0].QCType.Should().Be(QCTypeEnum.PQC);
        orders[0].Status.Should().Be(QCOrderStatusEnum.Pending);
    }

    [Fact]
    public async Task CreateAsync_IQCWithoutSupplier_ShouldThrow()
    {
        var command = new QCOrderCreateCommand
        {
            QCType = QCTypeEnum.IQC,
            ProductName = "产品A",
            SubmitQty = 100,
        };

        var act = () => QCOrderService.CreateAsync(command);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("供应商"));
    }

    // ============================================================
    // UpdateAsync
    // ============================================================

    [Fact]
    public async Task UpdateAsync_ShouldUpdatePendingOrder()
    {
        var order = new QCOrderEntity { Code = "QC001", QCType = QCTypeEnum.PQC,
            ProductName = "原名称", Status = QCOrderStatusEnum.Pending, SubmitQty = 50 };
        Db.Seed(order);

        var command = new QCOrderUpdateCommand
        {
            Id = order.Id,
            ProductName = "新名称",
            SubmitQty = 100,
        };

        var result = await QCOrderService.UpdateAsync(command);

        result.IsSuccess.Should().BeTrue();
        var updated = await Client.Queryable<QCOrderEntity>().FirstAsync(q => q.Id == order.Id);
        updated.ProductName.Should().Be("新名称");
    }

    [Fact]
    public async Task UpdateAsync_QualifiedOrder_ShouldThrow()
    {
        var order = new QCOrderEntity { Code = "QC001", QCType = QCTypeEnum.PQC,
            ProductName = "产品A", Status = QCOrderStatusEnum.Qualified, SubmitQty = 50 };
        Db.Seed(order);

        var command = new QCOrderUpdateCommand { Id = order.Id, ProductName = "新名称" };

        var act = () => QCOrderService.UpdateAsync(command);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("待检验"));
    }

    // ============================================================
    // DeleteAsync
    // ============================================================

    [Fact]
    public async Task DeleteAsync_ShouldDeletePendingOrder()
    {
        var order = new QCOrderEntity { Code = "QC001", QCType = QCTypeEnum.PQC,
            ProductName = "产品A", Status = QCOrderStatusEnum.Pending };
        Db.Seed(order);

        var result = await QCOrderService.DeleteAsync(order.Id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ShouldThrow()
    {
        var act = () => QCOrderService.DeleteAsync(99999);

        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("不存在"));
    }
}
