
using Hangfire;
using Microsoft.AspNetCore.Identity;
using SmartTask.Application;
using SmartTask.Identity;
using SmartTask.Identity.Contexts;
using SmartTask.Identity.Seeds;
using SmartTask.Persistence;
using SmartTask.Persistence.Contexts;
using SmartTask.Persistence.Services;
using SmartTask.Shared;

namespace SmartTask.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddIdentityInfrastructure(builder.Configuration);
            builder.Services.AddPersistenceInfrastructure(builder.Configuration);
            builder.Services.AddSharedInfrastructure(builder.Configuration);
            builder.Services.AddControllers();
            // Add Hangfire and its storage (SQL Server in this case)
            builder.Services.AddHangfire(config =>
                config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddHangfireServer();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddApplicationLayer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "SmartTasks API", Version = "v1" });

                // Add JWT support
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your valid JWT token."
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
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
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                // Seed Super Admin and default roles
                await DefaultSuperAdmin.SeedAsync(services);

                // Get required services for permission seeding
                var dbContext = services.GetRequiredService<IdentityContext>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("RolePermissionSeeder");

                try
                {
                    await RolePermissionSeeder.SeedAsync(dbContext, roleManager, logger);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred while seeding role permissions.");
                }
            }

                
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.UseHangfireDashboard(); // Optional: /hangfire

            // ✅ Schedule recurring jobs *AFTER* app is built
            RecurringJob.AddOrUpdate<OverdueTaskDetectorJob>(
                "overdue-task-checker",
                job => job.ExecuteAsync(),
                "*/5 * * * *"); // every 5 minutes

            app.Run();
        }
    }
}
