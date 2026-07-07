using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Common;

namespace TaxOmbud.Application.Interfaces.Repositories;

public interface IGenericRepository<T> where T : class
{
    #region Basic CRUD
    Task<T?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<T>> GetAllAsync(params string[] includeProperties);
    Task<T> AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity);
    Task RemoveAsync(T entity);
    Task RemoveAsync(Guid id);
    Task RemoveRangeAsync(IEnumerable<T> entities);
    Task DeleteRangeAsync(IEnumerable<T> entities);
    #endregion

    #region Query
    Task<T?> FindAsync(Expression<Func<T, bool>> expression);
    Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate);
    IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges);
    IQueryable<T> Query();
    #endregion

    #region Includes
    IQueryable<T> Include(params Expression<Func<T, object>>[] includeProperties);
    Task<IEnumerable<T>> FindAndIncludeAsync(Expression<Func<T, bool>> expression, params string[] includeProperties);
    #endregion

    #region Pagination
    Task<PagedResult<T>> GetPagedAsync(
        int page,
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        params string[] includeProperties);
    #endregion

    #region Count / Exists
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<T, bool>> expression);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> expression);
    #endregion

    #region Transactions
    Task<IDbContextTransaction> BeginTransactionAsync();
    #endregion

    #region Save
    Task<bool> SaveAsync();
    #endregion

    #region Raw SQL
    IQueryable<T> FromSqlRaw(string sql, params object[] parameters);
    #endregion
}
