using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Repositories.Models.QueryModels;
using System.Linq.Dynamic.Core;

namespace PRN232.LMS.Repositories.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly Prn232Lab1Context _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(Prn232Lab1Context context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(object id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<T?> GetByIdWithIncludesAsync(Expression<Func<T, bool>> predicate, params string[] includes)
    {
        IQueryable<T> query = _dbSet;
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return await query.FirstOrDefaultAsync(predicate);
    }

    public async Task<PRN232.LMS.Repositories.Models.QueryModels.PagedResult<object>> GetListAsync(QueryParameters parameters, Expression<Func<T, bool>>? searchPredicate = null)
    {
        try
        {
            IQueryable<T> query = _dbSet.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(parameters.Expand))
            {
                var includes = parameters.Expand.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            if (searchPredicate != null && !string.IsNullOrWhiteSpace(parameters.Search))
            {
                query = query.Where(searchPredicate);
            }

            if (!string.IsNullOrWhiteSpace(parameters.Sort))
            {
                var sortParams = parameters.Sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var sortBuilder = new List<string>();
                foreach (var sortParam in sortParams)
                {
                    if (sortParam.StartsWith("-"))
                    {
                        sortBuilder.Add($"{sortParam[1..]} descending");
                    }
                    else
                    {
                        sortBuilder.Add($"{sortParam} ascending");
                    }
                }
                if (sortBuilder.Count > 0)
                {
                    query = query.OrderBy(string.Join(", ", sortBuilder));
                }
            }

            var totalItems = await query.CountAsync();
            var page = parameters.Page < 1 ? 1 : parameters.Page;
            var pageSize = parameters.Size < 1 ? 10 : parameters.Size;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            IEnumerable<object> items;
            if (!string.IsNullOrWhiteSpace(parameters.Fields))
            {
                var fields = parameters.Fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var selectQuery = $"new({string.Join(", ", fields)})";
                items = await query.Select(selectQuery).ToDynamicListAsync();
            }
            else
            {
                items = await query.ToListAsync();
            }

            return new PRN232.LMS.Repositories.Models.QueryModels.PagedResult<object>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                Items = items
            };
        }
        catch (Exception ex) when (ex is System.Linq.Dynamic.Core.Exceptions.ParseException || ex is InvalidOperationException)
        {
            throw new ArgumentException($"Invalid query parameters: {ex.Message}", ex);
        }
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(object id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
