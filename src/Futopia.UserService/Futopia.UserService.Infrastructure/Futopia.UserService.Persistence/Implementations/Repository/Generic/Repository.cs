using Futopia.UserService.Application.Abstractions.Repository.Generic;
using Futopia.UserService.Domain.Entities.Common;
using Futopia.UserService.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace Futopia.UserService.Persistence.Implementations.Repository.Generic;
public class Repository<T> : IRepository<T> where T : BaseEntity, new()
{
    protected readonly AppDbContext _context;
    private readonly DbSet<T> _table;


    public Repository(AppDbContext context)
    {
        _context = context;
        _table = context.Set<T>();  

    }

    public async Task AddAsync(T entity)
    {
        await _table.AddAsync(entity);
    }

    public Task<bool> AnyAsync(Expression<Func<T, bool>> expression)
    {
        return _table.AnyAsync(expression);
    }

    public async Task DeleteAsync(T entity)
    {
        _table.Remove(entity);
    }

    public IQueryable<T> GetAll(
        Expression<Func<T, bool>>? expression = null,
        Expression<Func<T, object>>? orderExpression = null,
        int skip = 0,
        int take = 0,
        bool isDescending = false,
        bool isTracking = false,
        bool ignoreQuery = false,
        params string[]? includes)
    {
        IQueryable<T> query = _table;
        if (expression != null) query = query.Where(expression);
        if (includes != null) query = _getIncludes(query, includes);

        if (orderExpression != null)
            query = isDescending ? query.OrderByDescending(orderExpression) : query.OrderBy(orderExpression);

        query = query.Skip(skip);
        if (take != 0) query = query.Take(take);

        if (ignoreQuery)
            query = query.IgnoreQueryFilters();

        return isTracking ? query : query.AsNoTracking();

    }

    private IQueryable<T> _getIncludes(IQueryable<T> query, params string[] includes)
    {
        for (int i = 0; i < includes.Length; i++)
        {
            query = query.Include(includes[i]);
        }
        return query;
    }

    public async Task<T> GetByIdAsync(string id, params string[] includes)
    {
        IQueryable<T> query = _table;

        if (includes != null)
            query = _getIncludes(query, includes);

        return (await query.FirstOrDefaultAsync(x => x.Id == id))!;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        _table.Update(entity);
    }
}
