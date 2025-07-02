using SmartTask.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepositoryAsync<TaskItem> Tasks { get; }
        IGenericRepositoryAsync<Company> Companies { get; }
        IGenericRepositoryAsync<AuditLog> Audit { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
