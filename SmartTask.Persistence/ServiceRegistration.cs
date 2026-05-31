using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SmartTask.Application.Interfaces;
using SmartTask.Domain.Entities;
using SmartTask.Identity.Services;
using SmartTask.Identity.Services.SmartTasks.Infrastructure.Identity.Managers;
using SmartTask.Persistence.Contexts;
using SmartTask.Persistence.Repositories;
using SmartTask.Persistence.Services;
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
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<IEntityManagerAsync, EntityMangerAsync>();
            services.AddScoped<IAuditLogRepository, AuditRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
           
            // Register JwtService or token generator class
            services.AddScoped<JwtService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IShipBubbleService, ShipBubbleService>();
            services.AddScoped<IPaystackService, PaystackService>();
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddTransient<INotificationService, NotificationService>();
            services.AddTransient<IDbConnection>(sp =>
    new NpgsqlConnection(configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped(typeof(IGenericRepositoryAsync<>), typeof(GenericRepositoryAsync<>));
            services.Configure<AppSettings>(options => configuration.GetSection("AppSettings"));

        }
    }
}
    