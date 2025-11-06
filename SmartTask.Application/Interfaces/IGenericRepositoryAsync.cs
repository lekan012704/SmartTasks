using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Interfaces
{
    public interface IGenericRepositoryAsync<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<T> GetByIdAsync(Guid id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task<T> UpdateAsync(long id);
        Task DeleteAsync(T entity);
        Task<T> DeleteAsync(long id);
        Task<bool> TaskExistsAsync(string title, Guid companyId);
        Task<bool> CompanyExistsAsync(string companyName);
        IQueryable<T> GetQueryable();
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);


    }
}
