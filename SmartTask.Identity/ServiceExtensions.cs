using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartTask.Identity.Models;
using SmartTask.Identity.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using SmartTask.Domain.Entities;
using SmartTask.Identity.Contexts;
using Microsoft.EntityFrameworkCore;

namespace SmartTask.Identity
{
    public static class ServiceExtensions
    {
        public static void AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<IdentityContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName)));
            services.AddIdentity<ApplicationUser, IdentityRole>()
              .AddEntityFrameworkStores<IdentityContext>()
              .AddDefaultTokenProviders();

            // JWT Configuration
            var jwtSettings = configuration.GetSection("JwtSettings");
            services.Configure<JwtSettings>(jwtSettings);

            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
           .AddJwtBearer(options =>
           {
               options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = true,
                   ValidateAudience = true,
                   ValidateLifetime = true,
                   ValidateIssuerSigningKey = true,
                   ValidIssuer = jwtSettings["Issuer"],
                   ValidAudience = jwtSettings["Audience"],
                   IssuerSigningKey = new SymmetricSecurityKey(key)
               };
           });
            services.AddAuthorization();

            // Register JwtService or token generator class
            services.AddScoped<JwtService>();
        }

    }
}

