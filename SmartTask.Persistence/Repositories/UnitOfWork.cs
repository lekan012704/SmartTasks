using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
        private readonly ApplicationDbContext _appContext;

        public IGenericRepositoryAsync<TaskItem> Tasks { get; }
        public IGenericRepositoryAsync<Company> Companies { get; }
        public IGenericRepositoryAsync<AuditLog> Audit { get; }

        public UnitOfWork(ApplicationDbContext appContext)
        {
            _appContext = appContext;

            Tasks = new GenericRepositoryAsync<TaskItem>(_appContext);
            Companies = new GenericRepositoryAsync<Company>(_appContext);
            Audit = new GenericRepositoryAsync<AuditLog>(_appContext);
        }

        public async Task BeginTransactionAsync()
        {
            var transaction = await _appContext.Database.BeginTransactionAsync();
            await _appContext.Database.UseTransactionAsync(transaction.GetDbTransaction());
        }

        public async Task CommitTransactionAsync()
        {
            await _appContext.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _appContext.Database.RollbackTransactionAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            var changes1 = await _appContext.SaveChangesAsync();
            return changes1;
        }

        public void Dispose()
        {
            _appContext.Dispose();
        }
    }


}
