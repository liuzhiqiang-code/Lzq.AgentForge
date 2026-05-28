using Lzq.MES.Domain.Enums;
using Lzq.Core.Models;

namespace Lzq.MES.Application.Contracts.Queries;

/// <summary>产线分页查询</summary>
public record LinePageQuery : PagedRequest
{
    public long? WorkshopId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public EnableStatusEnum? Status { get; set; }
}
