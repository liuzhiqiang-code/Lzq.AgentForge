using Lzq.AI.Application.Contracts.Commands;
using Lzq.AI.Application.Contracts.Queries;
using Lzq.AI.Application.Contracts.Requests;
using Microsoft.AspNetCore.Http;

namespace Lzq.AI.Application.Contracts.IServices;

public interface IChatsService
{
    Task<ApiResult> PageAsync(ChatsPageQuery query);
    Task<ApiResult> ListAsync();
    Task<ApiResult> HistoryListAsync(long id);
    Task<ApiResult> ModelsAsync();
    Task<ApiResult> CreateAsync(ChatsCreateCommand command);
    Task<ApiResult> UpdateAsync(ChatsUpdateCommand command);
    Task<ApiResult> UpdateTitleAsync(ChatsUpdateTitleCommand command);
    Task<ApiResult> UpdateTopAsync(long id);
    Task<ApiResult> DeleteAsync(long id);
    Task<ApiResult> BatchDeleteAsync(List<long> ids);
    Task CompletionAsync(ChatsCompletionRequest input);
    Task<ApiResult> SpeechToTextAsync(HttpRequest request);
}