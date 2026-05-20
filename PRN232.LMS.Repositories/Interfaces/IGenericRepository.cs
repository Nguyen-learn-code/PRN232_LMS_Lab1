using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PRN232.LMS.Repositories.Models.QueryModels;

namespace PRN232.LMS.Repositories.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(object id);
    Task<T?> GetByIdWithIncludesAsync(Expression<Func<T, bool>> predicate, params string[] includes);
    Task<PagedResult<object>> GetListAsync(QueryParameters parameters, Expression<Func<T, bool>>? searchPredicate = null);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(object id);
}
