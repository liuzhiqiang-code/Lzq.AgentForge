using Lzq.BaseData.Domain.Enums;
using Lzq.BaseData.Domain.Entities;
using Lzq.Extensions.SqlSugar.SeedData;

namespace Lzq.BaseData.Domain.SeedDatas;

/// <summary>
/// 工厂种子数据
/// </summary>
public class FactorySeedData : BaseSeedData<FactoryEntity>
{
    public override List<FactoryEntity> GetSeedData()
    {
        return
        [
            new FactoryEntity
            {
                Id = 10001,
                Code = "FACTORY-01",
                Name = "松翰智造一厂",
                Address = "深圳市龙岗区松翰智造工业园1号",
                Status = EnableStatusEnum.Enabled,
            },
            new FactoryEntity
            {
                Id = 10002,
                Code = "FACTORY-02",
                Name = "松翰智造二工厂",
                Address = "东莞市清溪镇碧月新科技开发区",
                Status = EnableStatusEnum.Enabled,
            },
        ];
    }
}
