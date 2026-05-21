using ModelContextProtocol.Server;
using System.ComponentModel;

public class WeatherTools
{
    private readonly ILogger<WeatherTools>? _logger;

    public WeatherTools(ILogger<WeatherTools>? logger = null)
    {
        _logger = logger;
    }

    [McpServerTool]
    [Description("获取指定地区的实时天气概况，返回温度、天气状况、风力、湿度及预警等信息。")]
    public async Task<WeatherResult> GetWeather(
        [Description("需要查询天气的城市或地区名称。例如：Shenzhen")] string location)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            throw new ArgumentException("地点不能为空或空白字符串。", nameof(location));
        }

        _logger?.LogInformation("查询 {Location} 的天气", location);
        var weatherData = await FetchWeatherDataAsync(location);
        _logger?.LogInformation("{Location} 天气查询完成：{Summary}，{Temperature}°C",
            location, weatherData.Summary, weatherData.TemperatureCelsius);

        return weatherData;
    }

    private Task<WeatherResult> FetchWeatherDataAsync(string location)
    {
        // 根据城市返回不同的模拟数据
        var result = location switch
        {
            "Shenzhen" or "深圳" => new WeatherResult
            {
                Location = "深圳",
                TemperatureCelsius = 26,
                FeelsLikeCelsius = 28,
                Summary = "多云",
                Humidity = 70,
                WindDirection = "东南风",
                WindScale = "2-3级",
                WindSpeedKmh = 10.0,
                PrecipitationProbability = 40,
                Alert = "预计未来几小时可能有短暂阵雨，建议携带雨具",
                Timestamp = DateTimeOffset.UtcNow
            },
            "Beijing" or "北京" => new WeatherResult
            {
                Location = "北京",
                TemperatureCelsius = 8.5,
                FeelsLikeCelsius = 6.0,
                Summary = "晴间多云",
                Humidity = 30,
                WindDirection = "北风",
                WindScale = "3-4级",
                WindSpeedKmh = 18.0,
                PrecipitationProbability = 0,
                Alert = null,
                Timestamp = DateTimeOffset.UtcNow
            },
            _ => new WeatherResult
            {
                Location = location,
                TemperatureCelsius = 20.0,
                FeelsLikeCelsius = 20.0,
                Summary = "多云",
                Humidity = 55,
                WindDirection = "西北风",
                WindScale = "2级",
                WindSpeedKmh = 8.0,
                PrecipitationProbability = 10,
                Alert = null,
                Timestamp = DateTimeOffset.UtcNow
            }
        };

        return Task.FromResult(result);
    }
}

public class WeatherResult
{
    [Description("查询的城市或地区名称")]
    public string Location { get; set; } = string.Empty;

    [Description("当前温度，单位：摄氏度")]
    public double TemperatureCelsius { get; set; }

    [Description("体感温度，单位：摄氏度")]
    public double FeelsLikeCelsius { get; set; }

    [Description("天气状况描述，例如：晴、多云、雨等")]
    public string Summary { get; set; } = string.Empty;

    [Description("相对湿度百分比，范围 0-100")]
    public double Humidity { get; set; }

    [Description("风向描述，例如：东风、东南风等")]
    public string WindDirection { get; set; } = string.Empty;

    [Description("风力等级描述，例如：2-3级")]
    public string WindScale { get; set; } = string.Empty;

    [Description("风速，单位：公里/小时")]
    public double WindSpeedKmh { get; set; }

    [Description("降雨概率百分比，范围 0-100")]
    public int PrecipitationProbability { get; set; }

    [Description("天气预警或建议信息")]
    public string? Alert { get; set; }

    [Description("数据更新时间（UTC）")]
    public DateTimeOffset Timestamp { get; set; }
}
