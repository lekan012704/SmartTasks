using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartTask.Application.Interfaces;
using SmartTask.Identity.Services;
using SmartTask.Identity.Services.SmartTasks.Infrastructure.Identity.Managers;
using SmartTask.Persistence.Contexts;
using SmartTask.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Persistence
{
    public static class ServiceRegistration
    {
        public static void AddPersistenceInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<IEntityManagerAsync, EntityMangerAsync>();
            services.AddScoped<IAuditLogRepository, AuditRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
           
            // Register JwtService or token generator class
            services.AddScoped<JwtService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddTransient<IDbConnection>(sp =>
    new SqlConnection(configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped(typeof(IGenericRepositoryAsync<>), typeof(GenericRepositoryAsync<>));    


        }
    }
}
    