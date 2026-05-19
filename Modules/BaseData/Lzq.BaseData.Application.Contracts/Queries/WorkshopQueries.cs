using Lzq.BaseData.Domain.Enums;
using Lzq.Core.Models;

namespace Lzq.BaseData.Application.Contracts.Queries;

/// <summary>车间分页查询</summary>
public record WorkshopPageQuery : PagedRequest
{
    public long? FactoryId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public EnableStatusEnum? Status { get; set; }
}
