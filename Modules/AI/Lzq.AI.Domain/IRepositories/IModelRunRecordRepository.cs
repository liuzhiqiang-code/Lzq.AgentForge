using Lzq.Extensions.SqlSugar.Repository;
using Microsoft.Extensions.DependencyInjection;
using Lzq.AI.Domain.Entities;

namespace Lzq.AI.Domain.IRepositories;

public interface IModelRunRecordRepository : ISqlSugarRepository<ModelRunRecordEntity>, ITransientDependency
{

}