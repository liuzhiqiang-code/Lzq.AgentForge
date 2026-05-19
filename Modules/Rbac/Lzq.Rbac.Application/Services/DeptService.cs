using Lzq.Core.Models;
using Lzq.Rbac.Application.Contracts.Commands;
using Lzq.Rbac.Application.Contracts.Dtos;
using Lzq.Rbac.Application.Contracts.IServices;
using Lzq.Rbac.Application.Contracts.Queries;
using Lzq.Rbac.Domain.Entities;
using Lzq.Rbac.Domain.IRepositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using SqlSugar;

namespace Lzq.Rbac.Application.Services;

public class DeptService : ServiceBase, IDeptService
{
    public DeptService() : base("/api/v1/rbac/dept") { }
    private IDeptRepository DeptRepository => GetRequiredService<IDeptRepository>();

    [OpenApiTag("rbac/dept"), OpenApiOperation("获取部门分页列表", "")]
    [RoutePattern(pattern: "page", true)]
    public async Task<ApiResult> PageAsync([FromBody] DeptPageQuery query)
    {
        RefAsync<int> total = 0;
        var pageList = await DeptRepository.AsQueryable().ToPageListAsync(query.Page, query.PageSize, total);
        var result = pageList.Map<List<DeptViewDto>>();
        return ApiResult.Success(new PagedResponse<DeptViewDto>(result, total));
    }

    [OpenApiTag("rbac/dept"), OpenApiOperation("获取部门列表", "")]
    [RoutePattern(pattern: "list", true)]
    public async Task<ApiResult> ListAsync([FromBody] DeptListQuery query)
    {
        // 获取所有部门数据
        var allDepts = (await DeptRepository.GetListAsync())
            .ToList()
            .Map<List<DeptViewDto>>();

        // 构建树形结构
        return ApiResult.Success(BuildDeptTree(allDepts, null));
    }

    // 递归构建部门树
    private List<DeptViewDto> BuildDeptTree(List<DeptViewDto> allDepts, long? parentId)
    {
        return allDepts
            .Where(d => d.Pid == parentId)
            .Select(d => new DeptViewDto
            {
                Id = d.Id,
                Pid = d.Pid,
                Name = d.Name,
                Status = d.Status,
                Remark = d.Remark,
                Children = BuildDeptTree(allDepts, d.Id) // 递归处理子节点
            })
            .ToList();
    }

    [OpenApiTag("rbac/dept"), OpenApiOperation("增加部门", "")]
    [RoutePattern(pattern: "create", true)]
    public async Task<ApiResult> CreateAsync([FromBody] DeptCreateCommand command)
    {
        var entity = command.Map<DeptEntity>();
        await DeptRepository.InsertAsync(entity);
        return ApiResult.Success();
    }

    [OpenApiTag("rbac/dept"), OpenApiOperation("更新部门", "")]
    [RoutePattern(pattern: "update", true)]
    public async Task<ApiResult> UpdateAsync([FromBody] DeptUpdateCommand command)
    {
        var entity = command.Map<DeptEntity>();
        await DeptRepository.UpdateAsync(entity);
        return ApiResult.Success();
    }

    [OpenApiTag("rbac/dept"), OpenApiOperation("删除部门", "")]
    [RoutePattern(pattern: "delete/{id}", true)]
    public async Task<ApiResult> DeleteAsync(long id)
    {
        await DeptRepository.DeleteAsync(a => id.Equals(a.Id));
        return ApiResult.Success();
    }

    [OpenApiTag("rbac/dept"), OpenApiOperation("批量删除部门", "")]
    [RoutePattern(pattern: "batchDelete", true, HttpMethod = "Delete")]
    public async Task<ApiResult> BatchDeleteAsync([FromBody] List<long> ids)
    {
        await DeptRepository.DeleteAsync(a => ids.Contains(a.Id));
        return ApiResult.Success();
    }
}
