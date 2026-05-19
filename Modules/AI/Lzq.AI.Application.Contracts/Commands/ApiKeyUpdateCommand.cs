using FluentValidation;
using FluentValidation.Validators;
using Lzq.AI.Domain.Enums;

namespace Lzq.AI.Application.Contracts.Commands;

public record ApiKeyUpdateCommand
{
    /// <summary>
    /// 
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// key名称
    /// </summary>
    public string KeyName { get; set; }

    /// <summary>
    /// base_url
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = false;

    public List<string> SelectModel { get; set; } = [];
}
public class ApiKeyUpdateCommandValidator : MasaAbstractValidator<ApiKeyUpdateCommand>
{
    public ApiKeyUpdateCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("ID必须大于0");
    }
}