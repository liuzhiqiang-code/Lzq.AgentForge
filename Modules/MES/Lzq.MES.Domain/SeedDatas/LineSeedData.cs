using Lzq.MES.Domain.Enums;
using Lzq.MES.Domain.Entities;
using Lzq.Extensions.SqlSugar.SeedData;

namespace Lzq.MES.Domain.SeedDatas;

/// <summary>
/// 产线种子数据
/// </summary>
public class LineSeedData : BaseSeedData<LineEntity>
{
    public override List<LineEntity> GetSeedData()
    {
        return
        [
            // 冲压车间 (20001) 的产线
            new LineEntity { Id = 30001, Code = "LINE-01", Name = "冲压一号产线", WorkshopId = 20001, Status = EnableStatusEnum.Enabled },
            new LineEntity { Id = 30002, Code = "LINE-02", Name = "冲压二号产线", WorkshopId = 20001, Status = EnableStatusEnum.Enabled },

            // 焊接车间 (20002) 的产线
            new LineEntity { Id = 30003, Code = "LINE-03", Name = "焊接一号产线", WorkshopId = 20002, Status = EnableStatusEnum.Enabled },
            new LineEntity { Id = 30004, Code = "LINE-04", Name = "焊接二号产线", WorkshopId = 20002, Status = EnableStatusEnum.Enabled },

            // 涂装车间 (20003) 的产线
            new LineEntity { Id = 30005, Code = "LINE-05", Name = "底涂产线",   WorkshopId = 20003, Status = EnableStatusEnum.Enabled },
            new LineEntity { Id = 30006, Code = "LINE-06", Name = "面涂产线",   WorkshopId = 20003, Status = EnableStatusEnum.Enabled },

            // 总装车间 (20004) 的产线
            new LineEntity { Id = 30007, Code = "LINE-07", Name = "总装一号产线", WorkshopId = 20004, Status = EnableStatusEnum.Enabled },
            new LineEntity { Id = 30008, Code = "LINE-08", Name = "总装二号产线", WorkshopId = 20004, Status = EnableStatusEnum.Enabled },

            // 注塑车间 (20005) 的产线
            new LineEntity { Id = 30009, Code = "LINE-09", Name = "注塑一号产线", WorkshopId = 20005, Status = EnableStatusEnum.Enabled },
            new LineEntity { Id = 30010, Code = "LINE-10", Name = "注塑二号产线", WorkshopId = 20005, Status = EnableStatusEnum.Enabled },
        ];
    }
}
