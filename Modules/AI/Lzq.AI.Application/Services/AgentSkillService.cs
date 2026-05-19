using Lzq.AI.Application.Contracts.Commands;
using Lzq.AI.Application.Contracts.IServices;
using Lzq.AI.Application.Contracts.Requests;
using Lzq.Extensions.AI.AgentSkills;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;

namespace Lzq.AI.Application.Services;

public class AgentSkillService : ServiceBase, IAgentSkillService
{
    public AgentSkillService() : base("/api/v1/ai/agentSkill") { }
    private ISkillManager SkillManager => GetRequiredService<ISkillManager>();
    private ILogger<AgentSkillService> Logger => GetRequiredService<ILogger<AgentSkillService>>();

    [OpenApiTag("ai/agentSkill"), OpenApiOperation("获取列表", "")]
    [RoutePattern(pattern: "list", true)]
    public async Task<ApiResult> ListAsync()
    {
        return ApiResult.Success(SkillManager.GetSkills());
    }

    [OpenApiTag("ai/agentSkill"), OpenApiOperation("增加", "")]
    [RoutePattern(pattern: "create", true)]
    public async Task<ApiResult> CreateAsync([FromBody] AgentManageCreateCommand command)
    {
        //var entity = command.Map<AIAgentManageEntity>();
        //await AIAgentManageRepository.InsertAsync(entity);
        return ApiResult.Success();
    }

    [OpenApiTag("ai/agentSkill"), OpenApiOperation("更新", "")]
    [RoutePattern(pattern: "update", true)]
    public async Task<ApiResult> UpdateAsync([FromBody] AgentManageUpdateCommand command)
    {
        //var entity = command.Map<AIAgentManageEntity>();
        //await AIAgentManageRepository.UpdateAsync(entity);
        return ApiResult.Success();
    }

    [OpenApiTag("ai/agentSkill"), OpenApiOperation("删除", "")]
    [RoutePattern(pattern: "delete/{id}", true)]
    public async Task<ApiResult> DeleteAsync(long id)
    {
        //await AIAgentManageRepository.DeleteAsync(a => id.Equals(a.Id));
        return ApiResult.Success();
    }

    [OpenApiTag("ai/agentSkill"), OpenApiOperation("批量删除", "")]
    [RoutePattern(pattern: "batchDelete", true, HttpMethod = "Delete")]
    public async Task<ApiResult> BatchDeleteAsync([FromBody] List<long> ids)
    {
        //await AIAgentManageRepository.DeleteAsync(a => ids.Contains(a.Id));
        return ApiResult.Success();
    }

    [OpenApiTag("ai/agentSkill"), OpenApiOperation("执行", "")]
    [RoutePattern(pattern: "execute", true)]
    public async Task<ApiResult> ExecuteAsync([FromBody] ExecuteSkillRequest request)
    {
        try
        {
            var result = await SkillManager.ExecuteAsync(request.SkillName, request.ToolName, request.Arguments);
            if (result == null)
                return ApiResult.Fail("Not Found", 400);
            return ApiResult.Success(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "技能执行失败");
            return ApiResult.Fail("执行异常", 400);
        }
    }

    [OpenApiTag("ai/agentSkill"), OpenApiOperation("上传技能程序集", "")]
    [RoutePattern(pattern: "upload-plugin", true)]
    public async Task<ApiResult> UploadPluginAsync(HttpRequest request)
    {
        var form = await request.ReadFormAsync();
        var file = form.Files["file"];
        if (file == null || file.Length == 0)
            throw new UserFriendlyException("未找到上传的程序集文件");
        if (file.Length > 50 * 1024 * 1024)
            throw new UserFriendlyException("文件大小超过 50MB 限制");

        await SkillManager.UploadPluginAsync(file.FileName, file.OpenReadStream());
        return ApiResult.Success();
    }

    [OpenApiTag("ai/agentSkill"), OpenApiOperation("上传外部技能压缩包", "")]
    [RoutePattern(pattern: "upload-external", true)]
    public async Task<ApiResult> UploadExternalSkillZipAsync(HttpRequest request)
    {
        var form = await request.ReadFormAsync();
        var file = form.Files["file"];
        if (file == null || file.Length == 0)
            throw new UserFriendlyException("未找到上传的压缩包文件");
        if (file.Length > 100 * 1024 * 1024)
            throw new UserFriendlyException("文件大小超过 100MB 限制");

        await SkillManager.UploadExternalSkillZipAsync(file.FileName, file.OpenReadStream());
        return ApiResult.Success();
    }
}
