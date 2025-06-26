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
using SmartTask.Application.Interfaces;
using SmartTask.Identity.Services.SmartTasks.Infrastructure.Identity.Managers;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SmartTask.Application.Wrappers;
using Newtonsoft.Json;

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
            services.Configure<JwtSettings>(configuration.GetSection("JWTSettings"));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                 .AddJwtBearer(o =>
                 {
                     o.RequireHttpsMetadata = false;
                     o.SaveToken = false;
                     o.TokenValidationParameters = new TokenValidationParameters
                     {
                         ValidateIssuerSigningKey = true,
                         ValidateIssuer = true,
                         ValidateAudience = true,
                         ValidateLifetime = true,
                         ClockSkew = TimeSpan.Zero,
                         ValidIssuer = configuration["JWTSettings:Issuer"],
                         ValidAudience = configuration["JWTSettings:Audience"],
                         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWTSettings:Key"]))
                     };
                     o.Events = new JwtBearerEvents()
                     {
                         OnAuthenticationFailed = c =>
                         {
                             c.NoResult();
                             c.Response.StatusCode = 500;
                             c.Response.ContentType = "text/plain";
                             return c.Response.WriteAsync(c.Exception.ToString());
                         },
                         OnChallenge = context =>
                         {
                             context.HandleResponse();
                             context.Response.StatusCode = 401;
                             context.Response.ContentType = "application/json";
                             var result = JsonConvert.SerializeObject(new Response<string>("You are not Authorized"));
                             return context.Response.WriteAsync(result);
                         },
                         OnForbidden = context =>
                         {
                             context.Response.StatusCode = 403;
                             context.Response.ContentType = "application/json";
                             var result = JsonConvert.SerializeObject(new Response<string>("You are not authorized to access this resource"));
                             return context.Response.WriteAsync(result);
                         },
                     };
                 });

            // Register JwtService or token generator class
            services.AddScoped<JwtService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IPermissionService, PermissionService>();

        }

    }
}

