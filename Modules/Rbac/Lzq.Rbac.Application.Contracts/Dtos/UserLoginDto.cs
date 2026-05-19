namespace Lzq.Rbac.Application.Contracts.Dtos;

public class UserLoginDto
{
    private string v1;
    private string v2;

    public UserLoginDto(string v1, string v2)
    {
        this.v1 = v1;
        this.v2 = v2;
    }
}
