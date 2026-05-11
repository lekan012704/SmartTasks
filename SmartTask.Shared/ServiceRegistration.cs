using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartTask.Application.Interfaces;
using SmartTask.Domain.Entities;
using SmartTask.Shared.Helpers;
using SmartTask.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Shared
{
    public static class ServiceRegistration
    {
        public static void AddSharedInfrastructure(this IServiceCollection services, IConfiguration _config)
        {
            services.AddTransient<IAuthenticatedUserService, AuthenticatedUserService>();
            services.AddTransient<IEmailService, EmailService>();
            
            //services.Configure<MailSettings>(options => _config.GetSection("MailSettings"));

        }
    }
}
