using Lzq.BaseData.Domain.Entities;
using Lzq.Extensions.SqlSugar.SeedData;

namespace Lzq.BaseData.Domain.SeedDatas;

/// <summary>
/// 工序种子数据
/// </summary>
public class ProcessSeedData : BaseSeedData<ProcessEntity>
{
    public override List<ProcessEntity> GetSeedData()
    {
        return
        [
            // 冲压一号线 (30001) 工序
            new ProcessEntity { Id = 40001, Code = "PROC-01", Name = "板材上料",   LineId = 30001, Sequence = 1, StandardHours = 0.5m },
            new ProcessEntity { Id = 40002, Code = "PROC-02", Name = "模具冲压",   LineId = 30001, Sequence = 2, StandardHours = 2.0m },
            new ProcessEntity { Id = 40003, Code = "PROC-03", Name = "飞边清理",   LineId = 30001, Sequence = 3, StandardHours = 1.0m },

            // 焊接一号线 (30003) 工序
            new ProcessEntity { Id = 40004, Code = "PROC-04", Name = "点焊定位",   LineId = 30003, Sequence = 1, StandardHours = 1.0m },
            new ProcessEntity { Id = 40005, Code = "PROC-05", Name = "满焊加固",   LineId = 30003, Sequence = 2, StandardHours = 3.0m },
            new ProcessEntity { Id = 40006, Code = "PROC-06", Name = "打磨焊缝",   LineId = 30003, Sequence = 3, StandardHours = 1.5m },

            // 总装一号线 (30007) 工序
            new ProcessEntity { Id = 40007, Code = "PROC-07", Name = "部件预装",   LineId = 30007, Sequence = 1, StandardHours = 2.0m },
            new ProcessEntity { Id = 40008, Code = "PROC-08", Name = "主体组装",   LineId = 30007, Sequence = 2, StandardHours = 4.0m },
            new ProcessEntity { Id = 40009, Code = "PROC-09", Name = "终检调试",   LineId = 30007, Sequence = 3, StandardHours = 1.0m },
        ];
    }
}
