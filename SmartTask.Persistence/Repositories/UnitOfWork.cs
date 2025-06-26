using SmartTask.Application.Interfaces;
using SmartTask.Domain.Entities;
using SmartTask.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext _context;

        public IGenericRepositoryAsync<TaskItem> Tasks { get; }
        public IGenericRepositoryAsync<Company> Companies { get; }
        public IGenericRepositoryAsync<AuditLog> Audit { get; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Tasks = new GenericRepositoryAsync<TaskItem>(_context);
            Companies = new GenericRepositoryAsync<Company>(_context);
            Audit = new GenericRepositoryAsync<AuditLog>(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }

}
