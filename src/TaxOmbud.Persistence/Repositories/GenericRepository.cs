using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Domain.Common;
using TaxOmbud.Persistence.Data;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Persistence.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext _dbContext;
    protected readonly DbSet<T> _dbSet;
    protected readonly ILogger<GenericRepository<T>> _logger;

    public GenericRepository(ApplicationDbContext dbContext, ILogger<GenericRepository<T>> logger)
    {
        _dbContext = dbContext;
        _dbSet = _dbContext.Set<T>();
        _logger = logger;
    }

    #region Basic CRUD

    public virtual async Task<T?> GetByIdAsync(Guid id)
        => await _dbSet.FindAsync(id);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(params string[] includeProperties)
    {
        IQueryable<T> query = _dbSet;
        query = includeProperties.Aggregate(query, (current, prop) => current.Include(prop));
        return await query.AsNoTracking().ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        => await _dbSet.AddRangeAsync(entities);

    public virtual Task UpdateAsync(T entity)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public virtual Task RemoveAsync(T entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public virtual async Task RemoveAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity is not null) _dbSet.Remove(entity);
    }

    public virtual Task RemoveRangeAsync(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }

    public virtual Task DeleteRangeAsync(IEnumerable<T> entities)
    {
        _dbContext.Set<T>().RemoveRange(entities);
        return Task.CompletedTask;
    }

    #endregion

    #region Query

    public virtual async Task<T?> FindAsync(Expression<Func<T, bool>> expression)
        => await _dbSet.FirstOrDefaultAsync(expression);

    public virtual async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.Where(predicate).ToListAsync();

    public virtual IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges)
        => trackChanges ? _dbSet.Where(expression) : _dbSet.Where(expression).AsNoTracking();

    public virtual IQueryable<T> Query()
        => _dbSet.AsQueryable();

    #endregion

    #region Includes

    public virtual IQueryable<T> Include(params Expression<Func<T, object>>[] includeProperties)
    {
        IQueryable<T> query = _dbSet;
        foreach (var include in includeProperties)
            query = query.Include(include);
        return query;
    }

    public virtual async Task<IEnumerable<T>> FindAndIncludeAsync(
        Expression<Func<T, bool>> expression,
        params string[] includeProperties)
    {
        IQueryable<T> query = _dbSet;
        if (expression is not null) query = query.Where(expression);
        foreach (var include in includeProperties)
            query = query.Include(include);
        return await query.ToListAsync();
    }

    #endregion

    #region Pagination

    public virtual async Task<PagedResult<T>> GetPagedAsync(
        int page,
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        params string[] includeProperties)
    {
        try
        {
            IQueryable<T> query = _dbSet;

            if (filter is not null) query = query.Where(filter);
            query = includeProperties.Aggregate(query, (current, prop) => current.Include(prop));
            if (orderBy is not null) query = orderBy(query);

            int totalCount = await query.CountAsync();

            var items = await query
                .AsNoTracking()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>(items, totalCount, page, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged data for {EntityType}", typeof(T).Name);
            throw;
        }
    }

    #endregion

    #region Count / Exists

    public virtual async Task<int> CountAsync() => await _dbSet.CountAsync();

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> expression)
        => await _dbSet.CountAsync(expression);

    public virtual async Task<bool> ExistsAsync(Guid id)
        => await _dbSet.AnyAsync(e => EF.Property<Guid>(e, "Id") == id);

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> expression)
        => await _dbSet.AnyAsync(expression);

    #endregion

    #region Transactions

    public virtual async Task<IDbContextTransaction> BeginTransactionAsync()
        => await _dbContext.Database.BeginTransactionAsync();

    #endregion

    #region Save

    public virtual async Task<bool> SaveAsync()
        => await _dbContext.SaveChangesAsync() > 0;

    #endregion

    #region Raw SQL

    public virtual IQueryable<T> FromSqlRaw(string sql, params object[] parameters)
        => _dbSet.FromSqlRaw(sql, parameters);

    #endregion
}
