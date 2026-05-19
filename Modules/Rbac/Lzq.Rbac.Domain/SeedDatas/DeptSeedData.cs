using Lzq.Extensions.SqlSugar.SeedData;
using Lzq.Rbac.Domain.Entities;

namespace Lzq.Rbac.Domain.SeedDatas;

public class DeptSeedData : BaseSeedData<DeptEntity>
{
    public override List<DeptEntity> GetSeedData()
    {
        return new List<DeptEntity>
        {
            new DeptEntity
            {
                Id = 1,
                Pid = null,
                Name = "总公司",
                Status = Enums.EnableStatusEnum.Enabled,
                Remark = ""
            }
        };
    }
}
