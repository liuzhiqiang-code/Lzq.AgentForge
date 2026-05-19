namespace Lzq.QA.Application.Contracts.Dtos;

/// <summary>
/// 涓嶈壇鐜囩粺璁＄粨鏋淒TO锛堜緵Dashboard妯″潡浣跨敤锛?/// </summary>
public class DefectRateStatisticsResultDto
{
    /// <summary>閫佹鎬绘暟閲?/summary>
    public int SubmitQty { get; set; }

    /// <summary>鍚堟牸鏁伴噺</summary>
    public int QualifiedQty { get; set; }

    /// <summary>涓嶅悎鏍兼暟閲?/summary>
    public int UnqualifiedQty { get; set; }

    /// <summary>鍚堟牸鐜?%)</summary>
    public decimal QualifiedRate { get; set; }

    /// <summary>涓嶈壇鐜?%)</summary>
    public decimal DefectRate { get; set; }
}
