using Lzq.Core.Models;
using Lzq.MES.Domain.Enums;

namespace Lzq.MES.Application.Contracts.Queries;

/// <summary>
/// 璐ㄦ鍗曞垎椤垫煡璇?/// </summary>
public record QCOrderPageQuery : PagedRequest
{
    /// <summary>璐ㄦ鍗曠紪鍙?/summary>
    public string? Code { get; init; }

    /// <summary>璐ㄦ绫诲瀷锛?-IQC 2-PQC 3-OQC</summary>
    public QCTypeEnum? QCType { get; init; }

    /// <summary>鍏宠仈鍗曟嵁缂栧彿</summary>
    public string? RefCode { get; init; }

    /// <summary>渚涘簲鍟咺D</summary>
    public long? SupplierId { get; init; }

    /// <summary>渚涘簲鍟嗗悕绉?/summary>
    public string? SupplierName { get; init; }

    /// <summary>浜у搧ID</summary>
    public long? ProductId { get; init; }

    /// <summary>浜у搧鍚嶇О</summary>
    public string? ProductName { get; init; }

    /// <summary>璐ㄦ鍗曠姸鎬?/summary>
    public QCOrderStatusEnum? Status { get; init; }

    /// <summary>妫€楠屽憳ID</summary>
    public long? InspectorId { get; init; }

    /// <summary>妫€楠屾棩鏈燂紙璧凤級</summary>
    public DateTime? InspectDateFrom { get; init; }

    /// <summary>妫€楠屾棩鏈燂紙姝級</summary>
    public DateTime? InspectDateTo { get; init; }

    /// <summary>鍒涘缓鏃堕棿锛堣捣锛?/summary>
    public DateTime? CreateTimeFrom { get; init; }

    /// <summary>鍒涘缓鏃堕棿锛堟锛?/summary>
    public DateTime? CreateTimeTo { get; init; }
}
