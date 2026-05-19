using Lzq.Core.Models;
using Lzq.Temp.Application.Contracts.TestConfig.Commands;
using Lzq.Temp.Application.Contracts.TestConfig.Dto;
using Lzq.Temp.Application.Contracts.TestConfig.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Lzq.Temp.Application.Contracts.IServices;

public interface ITestConfigService : ITransientDependency
{
    Task<ApiResult<PagedResponse<TestConfigViewDto>>> PageAsync(TestConfigPageQuery query);

    Task<ApiResult<TestConfigViewDto>> GetAsync(long id);

    Task<ApiResult<long>> CreateAsync(TestConfigCreateCommand command);

    Task<ApiResult<bool>> UpdateAsync(TestConfigUpdateCommand command);

    Task<ApiResult<bool>> DeleteAsync(long id);
}
