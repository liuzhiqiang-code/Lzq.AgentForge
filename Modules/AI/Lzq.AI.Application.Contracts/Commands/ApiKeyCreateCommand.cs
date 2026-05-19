using FluentValidation.Validators;
using Lzq.AI.Domain.Enums;

namespace Lzq.AI.Application.Contracts.Commands;

public record ApiKeyCreateCommand
{
    /// <summary>
    /// 厂商名称
    /// </summary>
    public ProviderEnum Provider { get; set; }

    /// <summary>
    /// key名称
    /// </summary>
    public string KeyName { get; set; }

    /// <summary>
    /// 加密后的key值
    /// </summary>
    public string KeyValue { get; set; }

    /// <summary>
    /// base_url
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }
    public List<string> SelectModel { get; set; } = [];
}
public class ApiKeyCreateCommandValidator : MasaAbstractValidator<ApiKeyCreateCommand>
{
    public ApiKeyCreateCommandValidator()
    {
    }
}