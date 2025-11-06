using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartTask.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using SmartTask.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SmartTask.Application.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SmartTask.Application.Wrappers;
using Newtonsoft.Json;
using SmartTask.Persistence.Contexts;

namespace SmartTask.Domain
{
    // Note: Renamed from ServiceExtensionsa to ServiceExtensions
    public static class ServiceExtensions
    {
        public static void AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var key = Encoding.UTF8.GetBytes(configuration["JWTSettings:Key"]);

            // --- STEP 1: CALL AddIdentity FIRST ---
            // This sets up all the Identity services and defaults to cookie auth
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()  
                .AddDefaultTokenProviders();

            // JWT Configuration
            services.Configure<JwtSettings>(configuration.GetSection("JWTSettings"));

            // --- STEP 2: CALL AddAuthentication SECOND ---
            // This OVERRIDES the cookie defaults and sets them to JWT, which is what we want for an API
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
                           NameClaimType = ClaimTypes.NameIdentifier,
                           RoleClaimType = ClaimTypes.Role,
                           ValidateIssuerSigningKey = true,
                           ValidateIssuer = true,   
                           ValidateAudience = true,
                           ValidateLifetime = true,
                           ClockSkew = TimeSpan.Zero,
                           ValidIssuer = configuration["JWTSettings:Issuer"],
                           ValidAudience = configuration["JWTSettings:Audience"],
                           IssuerSigningKey = new SymmetricSecurityKey(key)
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
                               // This event now correctly handles 401
                               context.HandleResponse(); // Stop default redirects
                               context.Response.StatusCode = 401;
                               context.Response.ContentType = "application/json";
                               var result = JsonConvert.SerializeObject(new Response<string>("You are not Authorized"));
                               return context.Response.WriteAsync(result);
                           },
                           OnForbidden = context =>
                           {
                               // This event now correctly handles 403
                               context.Response.StatusCode = 403;
                               context.Response.ContentType = "application/json";
                               var result = JsonConvert.SerializeObject(new Response<string>("You are not authorized to access this resource"));
                               return context.Response.WriteAsync(result);
                           },
                       };
                   });
        }
    }
}
