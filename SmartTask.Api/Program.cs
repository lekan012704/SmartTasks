using Hangfire; // Make sure this is imported
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models; // Make sure this is imported
using SmartTask.Application;
using SmartTask.Application.Interfaces;
using SmartTask.Domain;
using SmartTask.Identity.Seeds;
using SmartTask.Identity.Services;
using SmartTask.Persistence;
using SmartTask.Persistence.Contexts;
using SmartTask.Persistence.Services;
using SmartTask.Shared;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace SmartTask.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. AddIdentityInfrastructure should handle AddIdentity.
            // If it doesn't, this is fine, but it's likely redundant.
            builder.Services.AddIdentityInfrastructure(builder.Configuration);
             
            //builder.Services.ConfigureApplicationCookie(options =>
            //{
            //    // Disable redirects for API requests
            //    options.Events.OnRedirectToLogin = context =>
            //    {
            //        if (context.Request.Path.StartsWithSegments("/api"))
            //        {
            //            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            //            return Task.CompletedTask;
            //        }

            //        context.Response.Redirect(context.RedirectUri);
            //        return Task.CompletedTask;
            //    };

            //    options.Events.OnRedirectToAccessDenied = context =>
            //    {
            //        if (context.Request.Path.StartsWithSegments("/api"))
            //        {
            //            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            //            return Task.CompletedTask;
            //        }

            //        context.Response.Redirect(context.RedirectUri);
            //        return Task.CompletedTask;
            //    };
            //});


            builder.Services.AddScoped<IPermissionService, PermissionService>();
            builder.Services.AddPersistenceInfrastructure(builder.Configuration);
            builder.Services.AddSharedInfrastructure(builder.Configuration);
            builder.Services.AddApplicationLayer(); // Moved with other custom services

            builder.Services.AddControllers();

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

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

            // 2. Uncomment and place HttpsRedirection first
            app.UseHttpsRedirection();
            //app.UseStatusCodePages(async context =>
            //{
            //    var response = context.HttpContext.Response;
            //    Console.WriteLine($"Response Code: {response.StatusCode}");
            //    await response.WriteAsync($"Status Code: {response.StatusCode}");
            //});

            // 3. UseRouting must come before UseCors and UseAuthentication
            app.UseRouting();

            // 4. THIS IS THE FIX: Uncommented and moved to the correct position
            app.UseCors(); 

            // 5. Authentication comes after CORS but before Authorization
            app.UseAuthentication();
            app.UseAuthorization();
            
            // 6. Secure the Hangfire dashboard
            app.MapHangfireDashboard()
               .RequireAuthorization(); // Requires any authenticated user

            app.MapControllers();

            // 7. Data seeding and Hangfire jobs should be in the same scope
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
                    // 8. Schedule recurring jobs inside the scope
                    var jobLogger = loggerFactory.CreateLogger("RecurringJobs");
                    RecurringJob.AddOrUpdate<OverdueTaskDetectorJob>(
                        "overdue-task-checker",
                        job => job.ExecuteAsync(),
                        "*/5 * * * *"); // every 5 minutes
                    jobLogger.LogInformation("Recurring jobs registered.");
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

