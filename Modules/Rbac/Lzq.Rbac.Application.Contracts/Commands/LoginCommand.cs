using FluentValidation;
using FluentValidation.Validators;
using Lzq.Extensions.Jwt.Models;

/*
 * @author : liuzhiqiang
 * @date : 2026-1-1
 * @desc : user
 */
namespace Lzq.Rbac.Application.Contracts.Commands;

public record LoginCommand
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    public string? Password { get; set; }
}
public class LoginCommandValidator : MasaAbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotNull().WithMessage("用户名不能为null")
            .NotEmpty().WithMessage("用户名不能为空");

        RuleFor(x => x.Password)
            .NotNull().WithMessage("密码不能为null")
            .NotEmpty().WithMessage("密码不能为空");
    }
}