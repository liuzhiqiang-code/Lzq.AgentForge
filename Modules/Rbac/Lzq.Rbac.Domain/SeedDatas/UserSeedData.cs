using Lzq.Extensions.SqlSugar.SeedData;
using Lzq.Rbac.Domain.Entities;

namespace Lzq.Rbac.Domain.SeedDatas;

public class UserSeedData : BaseSeedData<UserEntity>
{
    public override List<UserEntity> GetSeedData()
    {
        return new List<UserEntity>
        {
            new UserEntity
            {
                Id = 1,
                UserName = "admin",
                Password = "123456",
                Surname = "",
                GivenName = "",
                Email = "admin@qq.com",
                Phone = "",
                Age = 0,
                Sex = 0,
                Remark = "",
                DeptId = 1,
                Roles = new List<string> { "1" }
            }
        };
    }
}
