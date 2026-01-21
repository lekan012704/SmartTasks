using Hangfire; 
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; 
using SmartTask.Application;
using SmartTask.Application.Interfaces;
using SmartTask.Domain;
using SmartTask.Domain.Entities;
using SmartTask.Identity.Seeds;
using SmartTask.Identity.Services;
using SmartTask.Infrastructure.Hubs;
using SmartTask.Persistence;
using SmartTask.Persistence.Contexts;
using SmartTask.Shared;
using System.Text;

namespace SmartTask.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- Your existing services ---
            builder.Services.AddIdentityInfrastructure(builder.Configuration);
            builder.Services.AddScoped<IPermissionService, PermissionService>();
            builder.Services.AddPersistenceInfrastructure(builder.Configuration);
            builder.Services.AddSharedInfrastructure(builder.Configuration);
            builder.Services.AddApplicationLayer();

            builder.Services.AddControllers();
            builder.Services.AddSignalR();
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            builder.Services.AddHttpClient();
            builder.Services.AddHttpClient("ShipBubbleSettingsApi", client =>
            {
         
                client.BaseAddress = new Uri(builder.Configuration["ShipBubbleSettings:BaseUrl"]);
                var apiKey = builder.Configuration["ShipBubbleSettings:ApiKey"];
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            });
            builder.Services.Configure<ShipBubbleSettings>(builder.Configuration.GetSection("ShipBubbleSettings"));
            builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.Configure<PaystackSettings>(builder.Configuration.GetSection("PaystackSettings"));


            // Add Hangfire and its storage
            builder.Services.AddHangfire(config =>
                config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddHangfireServer();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "SmartTasks API", Version = "v1" });

                // Add JWT support
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your valid JWT token."
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            // Your order is correct here
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapHub<NotificationHub>("/hubs/notifications");

            // Secure the Hangfire dashboard
            app.MapHangfireDashboard()
               .RequireAuthorization(); // Requires any authenticated user

            app.MapControllers();

            // Data seeding
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();

                try
                {
                    // Seed Super Admin
                    var adminLogger = loggerFactory.CreateLogger("DefaultSuperAdmin");
                    await DefaultSuperAdmin.SeedAsync(services);
                    adminLogger.LogInformation("Super Admin seeding complete.");
                }
                catch (Exception ex)
                {
                    var adminLogger = loggerFactory.CreateLogger("DefaultSuperAdmin");
                    adminLogger.LogError(ex, "Error occurred while seeding Super Admin.");
                }

                try
                {
                    // Seed Roles and Permissions
                    var dbContext = services.GetRequiredService<ApplicationDbContext>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    var roleLogger = loggerFactory.CreateLogger("RolePermissionSeeder");
                    await RolePermissionSeeder.SeedAsync(dbContext, roleManager, roleLogger);
                    roleLogger.LogInformation("Role and Permission seeding complete.");
                }
                catch (Exception ex)
                {
                    var roleLogger = loggerFactory.CreateLogger("RolePermissionSeeder");
                    roleLogger.LogError(ex, "Error occurred while seeding role permissions.");
                }

                try
                {
                    // Your recurring jobs
                }
                catch (Exception ex)
                {
                    var jobLogger = loggerFactory.CreateLogger("RecurringJobs");
                    jobLogger.LogError(ex, "Error occurred while registering recurring jobs.");
                }
            }

            app.Run();
        }
    }
}