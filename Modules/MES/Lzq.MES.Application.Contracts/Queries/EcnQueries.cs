using Lzq.MES.Domain.Enums;

namespace Lzq.MES.Application.Contracts.Queries;

public record EcnPageQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Code { get; set; }
    public string? Title { get; set; }
    public EcnStatusEnum? Status { get; set; }
}

