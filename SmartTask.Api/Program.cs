using Hangfire;
using Hangfire.PostgreSql;
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

            // CORS — AllowCredentials() is required for SignalR, so we can't use AllowAnyOrigin()
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(
                            builder.Configuration["AllowedOrigins"] ?? "http://localhost:3000"
                          )
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            builder.Services.AddHttpClient();
            //builder.Services.AddHttpClient("ShipBubbleSettingsApi", client =>
            //{
            //    client.BaseAddress = new Uri(builder.Configuration["ShipBubbleSettings:BaseUrl"]);
            //    var apiKey = builder.Configuration["ShipBubbleSettings:ApiKey"];
            //    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            //});
            //builder.Services.Configure<ShipBubbleSettings>(builder.Configuration.GetSection("ShipBubbleSettings"));
            builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
            builder.Services.Configure<PaystackSettings>(builder.Configuration.GetSection("PaystackSettings"));

            // Hangfire with PostgreSQL storage (was UseSqlServerStorage)
            builder.Services.AddHangfire(config =>
                config.UsePostgreSqlStorage(c =>
                    c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));

            builder.Services.AddHangfireServer();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "SmartTasks API", Version = "v1" });

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
            app.UseSwagger();
            app.UseSwaggerUI();

            // Removed UseHttpsRedirection — Render handles HTTPS at the proxy level
            // Keeping it causes redirect loops on Render
            app.UseRouting();

            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapHub<NotificationHub>("/hubs/notifications");

            app.MapHangfireDashboard()
               .RequireAuthorization();

            app.MapControllers();

            // Data seeding
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();

                try
                {
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

            // Render injects PORT env variable — bind to it
            var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
            app.Run($"http://0.0.0.0:{port}");
        }
    }
}