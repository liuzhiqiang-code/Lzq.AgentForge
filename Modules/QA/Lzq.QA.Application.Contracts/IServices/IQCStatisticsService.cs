using Lzq.QA.Application.Contracts.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.QA.Application.Contracts.IServices;

/// <summary>
/// 质检统计服务接口（供Dashboard模块使用）
/// </summary>
public interface IQCStatisticsService : ITransientDependency
{
    /// <summary>
    /// 鑾峰彇鎸囧畾鏃堕棿鑼冨洿鐨勪笉鑹巼缁熻
    /// </summary>
    /// <param name="start">寮€濮嬫椂闂?/param>
    /// <param name="end">缁撴潫鏃堕棿</param>
    /// <param name="lineId">浜х嚎ID锛堝彲閫夛紝淇濈暀鎺ュ彛鎵╁睍鎬э級</param>
    /// <returns>涓嶈壇鐜囩粺璁＄粨鏋?/returns>
    Task<DefectRateStatisticsResultDto> GetDefectRateStatisticsAsync(DateTime start, DateTime end, long? lineId = null);
}
