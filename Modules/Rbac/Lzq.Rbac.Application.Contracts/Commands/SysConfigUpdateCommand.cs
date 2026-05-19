using FluentValidation.Validators;

namespace Lzq.Rbac.Application.Contracts.Commands;
public record SysConfigUpdateCommand
{
    public long Id { get; set; }
}
public class SysConfigUpdateCommandValidator : MasaAbstractValidator<SysConfigUpdateCommand>
{
    public SysConfigUpdateCommandValidator()
    {
        //RuleFor(user => user.Account).Letter();

        // WhenNotEmpty 的调用示例
        //_ = WhenNotEmpty(r => r.Phone, r => r.Phone, new PhoneValidator<RegisterUser>());
        //_ = WhenNotEmpty(r => r.Phone, new PhoneValidator<RegisterUser>());
        //_ = WhenNotEmpty(r => r.Phone, r => r.Phone, rule => rule.Phone());
        //_ = WhenNotEmpty(r => r.Phone, rule => rule.Phone());
    }
}