namespace Lzq.AI.Application.Contracts.Queries;

public record ModelConfigListQuery
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

    public ModelConfigListQuery()
    {
    }
}