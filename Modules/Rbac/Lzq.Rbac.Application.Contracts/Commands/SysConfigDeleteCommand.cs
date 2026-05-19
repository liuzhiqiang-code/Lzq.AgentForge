using FluentValidation.Validators;

namespace Lzq.Rbac.Application.Contracts.Commands;

public record SysConfigDeleteCommand
{
    public List<long> Ids { get; set; }
    public SysConfigDeleteCommand(List<long> ids)
    {
        Ids = ids;
    }
}
public class SysConfigDeleteCommandValidator : MasaAbstractValidator<SysConfigCreateCommand>
{
    public SysConfigDeleteCommandValidator()
    {
        //RuleFor(user => user.Account).Letter();

        // WhenNotEmpty 的调用示例
        //_ = WhenNotEmpty(r => r.Phone, r => r.Phone, new PhoneValidator<RegisterUser>());
        //_ = WhenNotEmpty(r => r.Phone, new PhoneValidator<RegisterUser>());
        //_ = WhenNotEmpty(r => r.Phone, r => r.Phone, rule => rule.Phone());
        //_ = WhenNotEmpty(r => r.Phone, rule => rule.Phone());
    }
}