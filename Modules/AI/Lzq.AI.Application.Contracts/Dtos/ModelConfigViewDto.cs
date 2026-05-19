using Lzq.AI.Domain.Enums;

namespace Lzq.AI.Application.Contracts.Dtos;

public class ModelConfigViewDto
{
    /// <summary>
    /// 
    /// </summary>
    public long? Id { get; set; }

    /// <summary>
    /// api_key_id
    /// </summary>
    public long? ApiKeyId { get; set; }

    /// <summary>
    /// 自定义模型名称
    /// </summary>
    public string? ConfigName { get; set; }

    /// <summary>
    /// 厂商模型名称
    /// </summary>
    public string? DisplayModelName { get; set; }

    /// <summary>
    /// 上下文长度
    /// </summary>
    public int? ContextLength { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }
    public ProviderEnum Provider { get; set; }
    public string KeyName { get; set; }
}