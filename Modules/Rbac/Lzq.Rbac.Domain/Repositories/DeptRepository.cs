using Lzq.Extensions.SqlSugar.Repository;
using Lzq.Rbac.Domain.Entities;
using Lzq.Rbac.Domain.IRepositories;

namespace Lzq.Rbac.Domain.Repositories;

public class DeptRepository()
    : SqlSugarRepository<DeptEntity>(), IDeptRepository
{

}