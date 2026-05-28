using Lzq.Core.Models;
using Lzq.MES.Domain.Enums;

namespace Lzq.MES.Application.Contracts.Queries;

/// <summary>
/// 涓嶈壇鍝佸垎椤垫煡璇?/// </summary>
public record DefectRecordPageQuery : PagedRequest
{
    /// <summary>璐ㄦ鍗曠紪鍙?/summary>
    public string? QCOrderCode { get; init; }

    /// <summary>宸ュ崟缂栧彿</summary>
    public string? WorkOrderCode { get; init; }

    /// <summary>浜у搧鍚嶇О</summary>
    public string? ProductName { get; init; }

    /// <summary>鎵瑰彿</summary>
    public string? BatchNo { get; init; }

    /// <summary>涓嶅悎鏍间唬鐮?/summary>
    public string? DefectCode { get; init; }

    /// <summary>涓嶈壇鍝佺姸鎬?/summary>
    public DefectStatusEnum? Status { get; init; }

    /// <summary>澶勭悊鏂瑰紡</summary>
    public DefectHandlingEnum? HandlingType { get; init; }

    /// <summary>鍒涘缓鏃堕棿锛堣捣锛?/summary>
    public DateTime? CreateTimeFrom { get; init; }

    /// <summary>鍒涘缓鏃堕棿锛堟锛?/summary>
    public DateTime? CreateTimeTo { get; init; }
}
