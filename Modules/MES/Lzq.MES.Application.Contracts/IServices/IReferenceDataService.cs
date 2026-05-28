using Lzq.MES.Application.Contracts.ReferenceData;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.MES.Application.Contracts.IServices;

/// <summary>
/// 基础数据引用服务接口
/// 用于 WorkOrder 模块跨模块获取 Line、Process 等基础数据
/// 遵循模块解耦原则：通过 Application.Contracts 层接口访问
/// </summary>
public interface IReferenceDataService : ITransientDependency
{
    /// <summary>
    /// 根据ID获取产线信息
    /// </summary>
    Task<LineSimpleDto?> GetLineByIdAsync(long lineId);

    /// <summary>
    /// 根据ID列表批量获取产线信息
    /// </summary>
    Task<List<LineSimpleDto>> GetLinesByIdsAsync(List<long> lineIds);

    /// <summary>
    /// 根据ID获取工序信息
    /// </summary>
    Task<ProcessSimpleDto?> GetProcessByIdAsync(long processId);

    /// <summary>
    /// 根据ID列表批量获取工序信息
    /// </summary>
    Task<List<ProcessSimpleDto>> GetProcessesByIdsAsync(List<long> processIds);

    /// <summary>
    /// 验证产线是否存在
    /// </summary>
    Task<bool> LineExistsAsync(long lineId);

    /// <summary>
    /// 验证工序是否存在
    /// </summary>
    Task<bool> ProcessExistsAsync(long processId);
}
