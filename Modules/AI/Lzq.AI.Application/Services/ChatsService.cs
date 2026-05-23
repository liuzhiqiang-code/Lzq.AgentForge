using Lzq.AI.Application.Contracts.Commands;
using Lzq.AI.Application.Contracts.Dtos;
using Lzq.AI.Application.Contracts.IServices;
using Lzq.AI.Application.Contracts.Queries;
using Lzq.AI.Application.Contracts.Requests;
using Lzq.AI.Application.Contracts.Responses;
using Lzq.AI.Domain.Consts;
using Lzq.AI.Domain.Entities;
using Lzq.AI.Domain.IRepositories;
using Lzq.Core.Interfaces;
using Lzq.Core.Models;
using Lzq.Extensions.AI;
using Lzq.Extensions.AI.Interfaces;
using Lzq.Extensions.AI.Provider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;
using SqlSugar;
using System.ClientModel;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lzq.AI.Application.Services;

public class ChatsService : ServiceBase, IChatsService
{
    public ChatsService() : base("/api/v1/ai/chats") { }
    private IHttpClientFactory HttpClientFactory => GetRequiredService<IHttpClientFactory>();
    private IConfiguration Configuration => GetRequiredService<IConfiguration>();
    private HttpContext HttpContext => GetRequiredService<IHttpContextAccessor>().HttpContext!;
    private ICurrentUser CurrentUser => GetRequiredService<ICurrentUser>();
    private IAIAgentRunner AIAgentRunner => GetRequiredService<IAIAgentRunner>();
    private ApiKeyService ApiKeyService => GetRequiredService<ApiKeyService>();
    private AgentManageService AgentManageService => GetRequiredService<AgentManageService>();
    private ILogger<ChatsService> Logger => GetRequiredService<ILogger<ChatsService>>();
    private IChatsRepository ChatsRepository => GetRequiredService<IChatsRepository>();
    private IModelRunRecordRepository ModelRunRecordRepository => GetRequiredService<IModelRunRecordRepository>();
    private IModelConfigRepository ModelConfigRepository => GetRequiredService<IModelConfigRepository>();

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [OpenApiTag("ai/chats"), OpenApiOperation("获取分页列表", "")]
    [RoutePattern(pattern: "page", true)]
    public async Task<ApiResult> PageAsync([FromBody] ChatsPageQuery query)
    {
        RefAsync<int> total = 0;
        var pageList = await ChatsRepository.AsQueryable().ToPageListAsync(query.Page, query.PageSize, total);
        var result = pageList.Map<List<ChatsViewDto>>();
        return ApiResult.Success(new PagedResponse<ChatsViewDto>(result, total));
    }

    [OpenApiTag("ai/chats"), OpenApiOperation("获取列表", "")]
    [RoutePattern(pattern: "list", true)]
    public async Task<ApiResult> ListAsync()
    {
        var list = await ChatsRepository.AsQueryable()
            .Where(a => !SqlFunc.IsNullOrEmpty(a.SessionId))
            .Where(a => !SqlFunc.IsNullOrEmpty(a.Title))
            .WhereIF(!CurrentUser.UserId.IsNullOrWhiteSpace(), a => a.Creator.Equals(CurrentUser.UserId.ToInt64()))
            .OrderByDescending(a => a.IsTop)
            .OrderByDescending(a => a.ModificationTime)
            .ToListAsync();
        return ApiResult.Success(list.Map<List<ChatsViewDto>>());
    }

    [OpenApiTag("ai/chats"), OpenApiOperation("获取对话历史消息", "")]
    [RoutePattern(pattern: "history/{id}", true)]
    public async Task<ApiResult> HistoryListAsync(long id)
    {
        var data = await ChatsRepository.AsQueryable()
            .Where(a => a.Id.Equals(id)).FirstAsync();
        if (data == null)
            return ApiResult.Fail("未找到对话消息", 400);

        var chatHistorys = await ChatsRepository.AsSugarClient().Queryable<AIChatHistoryEntity>()
            .Where(a => data.SessionId.Equals(a.SessionId))
            .OrderBy(a => a.CreationTime)
            .ToListAsync();

        // 分组：同一轮对话的 assistant + tool 合并到一起
        var dtos = GroupMessages(chatHistorys);

        return ApiResult.Success(dtos);
    }

    private List<ChatHistoryViewDto> GroupMessages(List<AIChatHistoryEntity> messages)
    {
        var result = new List<ChatHistoryViewDto>();
        List<AIChatHistoryEntity>? currentGroup = null;   // 一轮消息是一次user+ assistant/tool的组合，到下一个user成一个新的组合

        foreach (var msg in messages)
        {
            if (msg.Role == "user")
            {
                // 先关闭上一组
                if (currentGroup != null)
                {
                    result.Add(BuildAssistantDto(currentGroup));
                    currentGroup = null;
                }

                // 用户消息直接添加
                result.Add(new ChatHistoryViewDto
                {
                    Id = msg.Id,
                    Role = "user",
                    Content = msg.Content!.FirstOrDefault()?.Content ??"",
                    Segments = null
                });
            }
            else if (msg.Role is "assistant" or "tool")
            {
                // 收集到当前组
                currentGroup ??= new List<AIChatHistoryEntity>();
                currentGroup.Add(msg);
            }
        }

        // 最后一组
        if (currentGroup != null)
        {
            result.Add(BuildAssistantDto(currentGroup));
        }

        return result;
    }

    private ChatHistoryViewDto BuildAssistantDto(List<AIChatHistoryEntity> group)
    {
        // 取第一条 assistant 的 Id
        var firstAssistant = group.FirstOrDefault(m => m.Role == "assistant");
        var dto = new ChatHistoryViewDto
        {
            Id = firstAssistant?.Id ?? group[0].Id,
            Role = "assistant",
            Content = string.Concat(group.Where(m => m.Role == "assistant").Select(m => m.Content)),
            Segments = BuildSegments(group)
        };

        return dto;
    }

    private List<TimelineSegmentDto>? BuildSegments(List<AIChatHistoryEntity> group)
    {
        var segments = new List<TimelineSegmentDto>();
        var toolCallMap = new Dictionary<string, TimelineSegmentDto>();

        foreach (var msg in group)
        {
            if (msg == null || msg.Content == null || !msg.Content.Any()) continue;

            var eventArgs = msg.Content!;
            foreach (var item in eventArgs)
            {

                switch (item.EventType)
                {
                    case StreamingEventType.Thinking:
                        segments.Add(new TimelineSegmentDto
                        {
                            Type = "thinking",
                            Content = item.Content,
                            Collapsed = true
                        });
                        break;
                    case StreamingEventType.TextChunk:
                        segments.Add(new TimelineSegmentDto
                        {
                            Type = "message",
                            Content = item.Content
                        });
                        break;
                    case StreamingEventType.ToolCallStart:
                        var toolSeg = new TimelineSegmentDto
                        {
                            Type = "tool",
                            CallId = item.CallId,
                            ToolName = item.ToolName,
                            Arguments = item.ToolArguments,
                            Status = "running",
                            Collapsed = true
                        };
                        segments.Add(toolSeg);
                        if (!string.IsNullOrEmpty(item.CallId))
                        {
                            toolCallMap[item.CallId] = toolSeg; // 记录以便 tool_end 时查找
                        }
                        break;
                    case StreamingEventType.ToolCallEnd:
                        // 查找对应的 tool_start 并更新状态
                        if (!string.IsNullOrEmpty(item.CallId) && toolCallMap.TryGetValue(item.CallId, out var runningTool))
                        {
                            runningTool.Status = "done";
                            runningTool.Result = item.ToolResult;
                        }
                        else
                        {
                            // 如果找不到对应的 start，就作为独立的 tool 段
                            segments.Add(new TimelineSegmentDto
                            {
                                Type = "tool",
                                CallId = item.CallId,
                                ToolName = item.ToolName,
                                Result = item.ToolResult,
                                Status = "done",
                                Collapsed = true
                            });
                        }
                        break;
                    case StreamingEventType.EchartsStart:
                        var echartsSeg = new TimelineSegmentDto
                        {
                            Type = "echarts",
                            CallId = item.CallId,
                            ToolName = item.ToolName,
                            Arguments = item.ToolArguments,
                            Status = "running",
                            Collapsed = true
                        };
                        segments.Add(echartsSeg);
                        if (!string.IsNullOrEmpty(item.CallId))
                        {
                            toolCallMap[item.CallId] = echartsSeg; // 记录以便 tool_end 时查找
                        }
                        break;
                    case StreamingEventType.EchartsEnd:
                        // 查找对应的 tool_start 并更新状态
                        if (!string.IsNullOrEmpty(item.CallId) && toolCallMap.TryGetValue(item.CallId, out var runningEcharts))
                        {
                            runningEcharts.Status = "done";
                            runningEcharts.Result = item.ToolResult;
                            runningEcharts.ChartOption = string.IsNullOrEmpty(item.ToolResult) ? null :
                                JsonSerializer.Deserialize<EchartsOption>(item.ToolResult,_jsonOptions);
                        }
                        else
                        {
                            // 如果找不到对应的 start，就作为独立的 tool 段
                            segments.Add(new TimelineSegmentDto
                            {
                                Type = "echarts",
                                CallId = item.CallId,
                                ToolName = item.ToolName,
                                Result = item.ToolResult,
                                Status = "done",
                                Collapsed = true,
                                ChartOption = string.IsNullOrEmpty(item.ToolResult) ? null :
                                    JsonSerializer.Deserialize<EchartsOption>(item.ToolResult, _jsonOptions)
                            });
                        }
                        break;
                    default: break;
                }
            }
        }

        return segments.Count > 0 ? segments : null;
    }

    [OpenApiTag("ai/chats"), OpenApiOperation("获取可用模型", "")]
    [RoutePattern(pattern: "models", true)]
    public async Task<ApiResult> ModelsAsync()
    {
        var list = await ModelConfigRepository.AsQueryable()
            .LeftJoin<ApiKeyEntity>((a, b) => a.ApiKeyId == b.Id)
            .Where(a => a.IsEnabled)
            .Where((a, b) => b.IsEnabled)
            .Select((a, b) => new ModelViewDto
            {
                Id = a.Id,
                ConfigName = a.ConfigName,
                KeyName = b.KeyName
            })
            .ToListAsync();
        return ApiResult.Success(list);
    }

    [OpenApiTag("ai/chats"), OpenApiOperation("增加", "")]
    [RoutePattern(pattern: "create", true)]
    public async Task<ApiResult> CreateAsync([FromBody] ChatsCreateCommand command)
    {
        var entity = command.Map<ChatsEntity>();
        await ChatsRepository.InsertAsync(entity);
        return ApiResult.Success();
    }

    [OpenApiTag("ai/chats"), OpenApiOperation("更新", "")]
    [RoutePattern(pattern: "update", true)]
    public async Task<ApiResult> UpdateAsync([FromBody] ChatsUpdateCommand command)
    {
        var entity = command.Map<ChatsEntity>();
        await ChatsRepository.UpdateAsync(entity);
        return ApiResult.Success();
    }

    [OpenApiTag("ai/chats"), OpenApiOperation("更新标题", "")]
    [RoutePattern(pattern: "updateTitle", true)]
    public async Task<ApiResult> UpdateTitleAsync([FromBody] ChatsUpdateTitleCommand command)
    {
        var entity = await ChatsRepository.GetByIdAsync(command.Id);
        if (entity == null)
            return ApiResult.Fail("未找到对话消息", 400);
        entity.Title = command.Title;
        await ChatsRepository.UpdateAsync(entity);
        return ApiResult.Success();
    }


    [OpenApiTag("ai/chats"), OpenApiOperation("修改置顶信息", "")]
    [RoutePattern(pattern: "updateTop/{id}", true)]
    public async Task<ApiResult> UpdateTopAsync(long id)
    {
        var entity = await ChatsRepository.GetByIdAsync(id);
        if (entity == null)
            return ApiResult.Fail("未找到对话消息", 400);
        entity.IsTop = !entity.IsTop;
        await ChatsRepository.UpdateAsync(entity);
        return ApiResult.Success();
    }

    [OpenApiTag("ai/chats"), OpenApiOperation("删除", "")]
    [RoutePattern(pattern: "delete/{id}", true)]
    public async Task<ApiResult> DeleteAsync(long id)
    {
        await ChatsRepository.DeleteAsync(a => id.Equals(a.Id));
        return ApiResult.Success();
    }

    [OpenApiTag("ai/chats"), OpenApiOperation("批量删除", "")]
    [RoutePattern(pattern: "batchDelete", true, HttpMethod = "Delete")]
    public async Task<ApiResult> BatchDeleteAsync([FromBody] List<long> ids)
    {
        await ChatsRepository.DeleteAsync(a => ids.Contains(a.Id));
        return ApiResult.Success();
    }

    [OpenApiTag("ai/chats"), OpenApiOperation("对话补全", "")]
    [RoutePattern(pattern: "completion", true)]
    public async Task CompletionAsync([FromBody] ChatsCompletionRequest input)
    {
        SetupSSEHeaders();
        CancellationToken cancellationToken = HttpContext.RequestAborted;

        try
        {
            // 1. 初始化主表记录
            var aiSetting = await ApiKeyService.GetAISettingAsync(input.AIModelConfigId);
            var agentModel = await AgentManageService.GetAIAgentModel(input.AIAgentName);
            var chatsEntity = await InitializeChatAsync(input, aiSetting, agentModel, cancellationToken);
            long chatId = chatsEntity.Id;

            // 1. 执行流式对话并记录到 ModelRunRecord
            var (completionResult, chatAgentModel) = await ProcessCompletionAsync(input, aiSetting, agentModel, chatsEntity, cancellationToken);
            if (completionResult == null)
            {
                await WriteSseEventAsync("error", new { v = "AI 响应失败" });
                return;
            }

            // 2. 推送用量和状态
            await SendStatusEventsAsync(completionResult);

            // 3. 新对话生成对话标题
            if (chatsEntity.Title.IsNullOrWhiteSpace())
                chatsEntity.Title = await GenerateTitleAsync(completionResult, aiSetting, chatsEntity, cancellationToken);

            // 推送对话信息
            await WriteSseEventAsync("aiChats", new { content = chatsEntity.Title, aiAgentName = chatsEntity.AIAgentName, chatId = chatsEntity.Id });

            // 4. 持久化对话及历史记录
            await UpdateChatEntityAsync(chatsEntity, completionResult);

            // 5. 通知前端更新会话并关闭流
            await WriteSseEventAsync("update_session", new { updated_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds() });
            await WriteSseEventAsync("close", new { click_behavior = "none", auto_resume = false });
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("SSE 连接被客户端取消");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "SSE 对话流程异常");
            // 尝试发送错误事件（忽略可能已经断开的连接）
            try { await WriteSseEventAsync("error", new { v = "内部服务器错误" }); } catch { }
        }
        finally
        {
            await HttpContext.Response.CompleteAsync();
        }
    }

    private void SetupSSEHeaders()
    {
        HttpContext.Response.ContentType = "text/event-stream";
        HttpContext.Response.Headers.CacheControl = "no-cache";
        HttpContext.Response.Headers.Connection = "keep-alive";
        HttpContext.Response.Headers["X-Accel-Buffering"] = "no";
    }

    private async Task<ChatsEntity> InitializeChatAsync(
        ChatsCompletionRequest input, AISetting aiSetting, AIAgentModel agentModel, CancellationToken ct)
    {
        ChatsEntity chat;
        if (input.ChatsId.HasValue && input.ChatsId.Value > 0)
        {
            chat = await ChatsRepository.GetFirstAsync(a => a.Id == input.ChatsId.Value);
            if (chat == null)
            {
                // 传入的 ID 无效，按新建处理（避免异常）
                chat = CreateNewChatEntity(input, aiSetting, agentModel);
                await ChatsRepository.InsertAsync(chat);
            }
        }
        else
        {
            chat = CreateNewChatEntity(input, aiSetting, agentModel);
            await ChatsRepository.InsertAsync(chat);
        }

        return chat;
    }

    private ChatsEntity CreateNewChatEntity(ChatsCompletionRequest input, AISetting aiSetting, AIAgentModel agentModel)
    {
        return new ChatsEntity
        {
            Title = "",
            ChatClient = aiSetting.ConfigId,
            AIAgentName = input.AIAgentName ?? "",
            // SessionId = Guid.NewGuid().ToString("N"),
            SelectedSkills = agentModel.SelectedSkills  //新建冗余绑定Skills数据
        };
    }

    private async Task UpdateChatEntityAsync(ChatsEntity chat, ChatContentDto completionResult)
    {
        chat.LastMessage = completionResult.Content;
        chat.ChatClient ??= completionResult.ChatClient;  // 若新建时未填，补全
        chat.AIAgentName ??= completionResult.AIAgentName ?? "默认";
        await ChatsRepository.UpdateAsync(chat);
    }

    /// <summary>
    /// 执行流式对话，返回对话结果（ChatContentDto）和使用的智能体模型
    /// </summary>
    private async Task<(ChatContentDto? Content, AIAgentModel AgentModel)> ProcessCompletionAsync(
        ChatsCompletionRequest input, AISetting aiSetting, AIAgentModel agentModel, ChatsEntity chatsEntity, CancellationToken ct)
    {
        agentModel.SelectedSkills = chatsEntity.SelectedSkills; // 使用对话绑定的工具
        var entity = new ModelRunRecordEntity
        {
            ChatClient = aiSetting.ConfigId,
            AIAgentModel = agentModel,
            AIAgentName = agentModel.Name,
            Instructions = agentModel.ChatOptions?.Instructions ?? "",
            Prompt = input.Prompt,
        };

        var stopwatch = Stopwatch.StartNew();
        try
        {
            string? sessionDbKey = null;
            if (!chatsEntity.SessionId.IsNullOrWhiteSpace())
                sessionDbKey = chatsEntity.SessionId;

            (var fullContent, sessionDbKey) = await AIAgentRunner.RunStreamingAsync(
                aiSetting,
                agentModel,
                input.Prompt,
                async args =>
                {
                    // SSE 标准输出
                    await WriteSseEventAsync(args.EventType switch
                    {
                        StreamingEventType.Thinking => "thinking",
                        StreamingEventType.TextChunk => "message",
                        StreamingEventType.ToolCallStart => "tool_start",
                        StreamingEventType.ToolCallEnd => "tool_end",
                        StreamingEventType.EchartsStart => "echarts_start",
                        StreamingEventType.EchartsEnd => "echarts_end",
                        _ => "unknown"
                    }, new
                    {
                        v = args.Content,
                        toolName = args.ToolName,
                        toolArgs = args.ToolArguments,
                        toolResult = args.ToolResult,
                        callId = args.CallId,
                        chartOption = (!string.IsNullOrEmpty(args.ToolResult) && args.EventType == StreamingEventType.EchartsEnd) ?
                            JsonSerializer.Deserialize<EchartsOption>(args.ToolResult,_jsonOptions) : null,
                    });
                }, sessionDbKey);
            chatsEntity.SessionId = sessionDbKey;

            entity.IsSuccess = true;
            entity.Content = fullContent;
        }
        catch (Exception ex)
        {
            string displayMessage = ex.Message;
            if (ex is ClientResultException rex)
            {
                // 识别 DeepSeek 推理回传错误
                if (displayMessage.Contains("reasoning_content") && displayMessage.Contains("passed back"))
                {
                    displayMessage = "【框架兼容性提示】检测到 DeepSeek 思考模型触发了工具调用。由于当前框架尚未实现 reasoning_content 的自动回传，请求已被 API 拦截。请尝试切换至非思考模型（如 deepseek-chat）。";
                }
            }

            await WriteSseEventAsync("error", new { v = displayMessage });
            entity.IsSuccess = false;
            entity.ErrorMessage = ex.Message;
            Logger.LogError(ex, "调用大模型失败");
        }
        finally
        {
            stopwatch.Stop();
            entity.DurationMs = stopwatch.ElapsedMilliseconds;
            await ModelRunRecordRepository.InsertAsync(entity);
        }

        var result = entity.IsSuccess ? entity.Map<ChatContentDto>() : null;
        return (result, agentModel);
    }

    /// <summary>生成对话标题（使用独立模型调用）</summary>
    private async Task<string> GenerateTitleAsync(ChatContentDto completionResult, AISetting setting, ChatsEntity chat, CancellationToken ct)
    {
        var prompt = $"用户：{completionResult.Prompt}，Agent：{completionResult.Content}  结合上下文返回一个标题 10字以内"; // 可结合 content 优化
        var entity = new ModelRunRecordEntity();
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var (content, _) = await AIAgentRunner.RunAsync(setting, AgentConst.TITLE, prompt);

            entity.ChatClient = setting.ConfigId;
            entity.AIAgentModel = AgentConst.TITLE;
            entity.AIAgentName = AgentConst.TITLE.Name;
            entity.Instructions = AgentConst.TITLE.ChatOptions?.Instructions ?? "";
            entity.Prompt = prompt;

            entity.IsSuccess = true;
            entity.Content = content.Text;
            stopwatch.Stop();
            entity.DurationMs = stopwatch.ElapsedMilliseconds;
            await ModelRunRecordRepository.InsertAsync(entity);

            string title = content?.Text?.Trim() ?? "新对话";
            return title;
        }
        catch (Exception ex)
        {
            entity.IsSuccess = false;
            entity.ErrorMessage = ex.Message;
            Logger.LogError(ex, "标题生成失败");
            stopwatch.Stop();
            entity.DurationMs = stopwatch.ElapsedMilliseconds;
            await ModelRunRecordRepository.InsertAsync(entity);

            await WriteSseEventAsync("title", new { content = "新对话", chatId = chat.Id });
            return "新对话";
        }
    }

    private async Task SendStatusEventsAsync(ChatContentDto completionResult)
    {
        var statusData = new
        {
            p = "response/status",
            o = "SET",
            v = "FINISHED"
        };
        await WriteSseEventAsync("message", statusData);
    }

    private async Task WriteSseEventAsync(string eventName, object data)
    {
        // 将对象序列化为标准 JSON：{"v": "你好"}
        var jsonString = System.Text.Json.JsonSerializer.Serialize(data, _jsonOptions);
        await HttpContext.Response.WriteAsync($"{eventName}: {jsonString}\n\n");
        await HttpContext.Response.Body.FlushAsync();
    }

    [AllowAnonymous]
    [OpenApiTag("ai/chats"), OpenApiOperation("语音转文字", "")]
    [RoutePattern(pattern: "speech-to-text", true)]
    public async Task<ApiResult> SpeechToTextAsync(HttpRequest request)
    {
        // 1. 获取上传文件
        var form = await request.ReadFormAsync();
        var formFile = form.Files["file"];
        if (formFile == null || formFile.Length == 0)
        {
            throw new UserFriendlyException("未找到上传的音频文件");
        }

        // 2. 校验文件（保持原样）
        if (formFile.Length > 25 * 1024 * 1024)
        {
            throw new UserFriendlyException("文件大小超过 25MB 限制");
        }

        var httpClient = HttpClientFactory.CreateClient("SiliconFlowClient");
        var keySecret = Configuration.GetSection("AIKeySecret:SiliconFlow").Get<string>() ?? "";
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", keySecret);

        using var httpForm = new MultipartFormDataContent();
        // 直接使用 IFormFile 的流
        var fileStream = formFile.OpenReadStream();
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(formFile.ContentType);
        httpForm.Add(fileContent, "file", formFile.FileName);
        httpForm.Add(new StringContent("FunAudioLLM/SenseVoiceSmall"), "model");

        var response = await httpClient.PostAsync("https://api.siliconflow.cn/v1/audio/transcriptions", httpForm);
        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var textResult = await response.Content.ReadFromJsonAsync<TranscriptionsResponse>();
            return ApiResult.Success<string>(textResult!.Text);
        }
        else
        {
            return ApiResult.Fail("语音转文字失败");
        }
    }
}
