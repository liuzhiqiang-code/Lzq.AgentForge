namespace Lzq.QA.Application.Contracts.Commands;

/// <summary>
/// 鏇存柊璐ㄦ鍗曞懡浠?/// </summary>
public record QCOrderUpdateCommand
{
    /// <summary>璐ㄦ鍗旾D</summary>
    public long Id { get; init; }

    /// <summary>鍏宠仈鍗曟嵁缂栧彿</summary>
    public string? RefCode { get; init; }

    /// <summary>渚涘簲鍟咺D锛圛QC鏃朵娇鐢級</summary>
    public long? SupplierId { get; init; }

    /// <summary>渚涘簲鍟嗗悕绉?/summary>
    public string? SupplierName { get; init; }

    /// <summary>浜у搧ID</summary>
    public long? ProductId { get; init; }

    /// <summary>浜у搧鍚嶇О</summary>
    public string? ProductName { get; init; }

    /// <summary>浜у搧瑙勬牸/鍨嬪彿</summary>
    public string? ProductSpec { get; init; }

    /// <summary>鎵瑰彿/鎵规</summary>
    public string? BatchNo { get; init; }

    /// <summary>閫佹鏁伴噺</summary>
    public int SubmitQty { get; init; }

    /// <summary>璐ㄦ鏍囧噯/渚濇嵁</summary>
    public string? QCStandard { get; init; }

    /// <summary>澶囨敞</summary>
    public string? Remark { get; init; }
}
