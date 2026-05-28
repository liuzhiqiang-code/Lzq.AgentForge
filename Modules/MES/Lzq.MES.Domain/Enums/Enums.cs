namespace Lzq.MES.Domain.Enums;

/// <summary>
/// 閫氱敤瀹炰綋鍚敤鐘舵€?/// </summary>
public enum EnableStatusEnum
{
    /// <summary>绂佺敤</summary>
    Disabled = 0,
    /// <summary>鍚敤</summary>
    Enabled = 1,
}

/// <summary>
/// 鐐规缁撴灉
/// </summary>
public enum CheckResultEnum
{
    /// <summary>姝ｅ父</summary>
    Normal = 0,
    /// <summary>寮傚父</summary>
    Abnormal = 1,
    /// <summary>宸插鐞?/summary>
    Resolved = 2,
}

/// <summary>
/// 璁￠噺鍗曚綅鍒嗙被
/// </summary>
public enum UnitCategoryEnum
{
    /// <summary>鏁伴噺锛堜釜/鍙?浠?濂楋級</summary>
    Count = 0,
    /// <summary>閲嶉噺锛坘g/g/t锛?/summary>
    Weight = 1,
    /// <summary>浣撶Н/瀹圭Н锛圠/ml/m鲁锛?/summary>
    Volume = 2,
    /// <summary>闀垮害锛坢/cm/mm锛?/summary>
    Length = 3,
    /// <summary>鍏朵粬</summary>
    Other = 99,
}

/// <summary>
/// 鐗╂枡鐘舵€?/// </summary>
public enum MaterialStatusEnum
{
    /// <summary>鍚敤</summary>
    Enabled = 0,
    /// <summary>绂佺敤</summary>
    Disabled = 1,
    /// <summary>鍋滅敤/娣樻卑</summary>
    Obsolete = 2,
}

/// <summary>
/// BOM 鐘舵€?/// </summary>
public enum BomStatusEnum
{
    /// <summary>鑽夌</summary>
    Draft = 0,
    /// <summary>宸插彂甯?/summary>
    Released = 1,
    /// <summary>宸插簾寮?/summary>
    Obsolete = 2,
    /// <summary>寰呬慨璁紙寮曠敤鐗╂枡宸插仠鐢?娣樻卑锛?/summary>
    RevisionPending = 3,
}

/// <summary>
/// ECN 鍙樻洿绫诲瀷
/// </summary>
public enum EcnChangeTypeEnum
{
    /// <summary>鐗╂枡</summary>
    Material = 0,
    /// <summary>BOM</summary>
    Bom = 1,
}

/// <summary>
/// ECN 鐘舵€?/// </summary>
public enum EcnStatusEnum
{
    /// <summary>鑽夌</summary>
    Draft = 0,
    /// <summary>寰呭鎵?/summary>
    Pending = 1,
    /// <summary>宸叉壒鍑?/summary>
    Approved = 2,
    /// <summary>宸叉墽琛?/summary>
    Executed = 3,
    /// <summary>宸茬‘璁?/summary>
    Confirmed = 4,
    /// <summary>宸插彇娑?/summary>
    Cancelled = 5,
}


