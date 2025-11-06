using Microsoft.EntityFrameworkCore;
using SmartTask.Application.Interfaces;
using SmartTask.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Persistence.Repositories
{
   public class GenericRepositoryAsync<T> :IGenericRepositoryAsync<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        public GenericRepositoryAsync(ApplicationDbContext context)
        {
            _context = context;
        }
        public virtual async Task<T> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }
        public virtual async Task<T> GetByIdAsync(Guid id)
        {
            return await _context.Set<T>().FindAsync(id);
        }
        public virtual async Task<T> AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            return entity;
        }
        public virtual async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }
        public virtual async Task UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
        }
        public virtual async Task<T> UpdateAsync(long id)
        {
            var entity = await GetByIdAsync((int)id);

            if (entity != null)
            {
               _context.Entry(entity).State = EntityState.Modified;
                return entity;
            }
            else
            {
                return null;
            }
        }
        public virtual async Task DeleteAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
        }
        public virtual async Task<T> DeleteAsync(long id)
        {
            var entity = await GetByIdAsync((int)id);
            if (entity != null)
            {
                _context.Set<T>().Remove(entity);
                return entity;
            }
            else
            {
                return null;
            }
        }
    
        public async Task<bool> CompanyExistsAsync(string companyName)
        {
            return await _context.Company.AnyAsync(c => c.Name == companyName);
        }
        public IQueryable<T> GetQueryable()
        {
            return _context.Set<T>().AsQueryable();
        }
        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(predicate);
        }

    }
}
