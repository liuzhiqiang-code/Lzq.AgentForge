using Lzq.MES.Domain.Enums;
using Lzq.Core.Models;

namespace Lzq.MES.Application.Contracts.Queries;

/// <summary>工序分页查询</summary>
public record ProcessPageQuery : PagedRequest
{
    public long? LineId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public EnableStatusEnum? Status { get; set; }
}
