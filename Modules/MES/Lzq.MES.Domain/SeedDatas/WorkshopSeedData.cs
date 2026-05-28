using Lzq.MES.Domain.Enums;
using Lzq.MES.Domain.Entities;
using Lzq.Extensions.SqlSugar.SeedData;

namespace Lzq.MES.Domain.SeedDatas;

/// <summary>
/// 车间种子数据
/// </summary>
public class WorkshopSeedData : BaseSeedData<WorkshopEntity>
{
    public override List<WorkshopEntity> GetSeedData()
    {
        return
        [
            new WorkshopEntity
            {
                Id = 20001,
                Code = "WS-A01",
                Name = "冲压车间",
                FactoryId = 10001,
                Manager = "张伟",
                Status = EnableStatusEnum.Enabled,
            },
            new WorkshopEntity
            {
                Id = 20002,
                Code = "WS-A02",
                Name = "焊接车间",
                FactoryId = 10001,
                Manager = "李强",
                Status = EnableStatusEnum.Enabled,
            },
            new WorkshopEntity
            {
                Id = 20003,
                Code = "WS-A03",
                Name = "涂装车间",
                FactoryId = 10001,
                Manager = "王芳",
                Status = EnableStatusEnum.Enabled,
            },
            new WorkshopEntity
            {
                Id = 20004,
                Code = "WS-A04",
                Name = "总装车间",
                FactoryId = 10001,
                Manager = "赵明",
                Status = EnableStatusEnum.Enabled,
            },
            new WorkshopEntity
            {
                Id = 20005,
                Code = "WS-B01",
                Name = "注塑车间",
                FactoryId = 10002,
                Manager = "陈丽",
                Status = EnableStatusEnum.Enabled,
            },
        ];
    }
}
