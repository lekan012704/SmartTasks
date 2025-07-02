using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SmartTask.Application.Command;
using SmartTask.Application.Command.Task;
using SmartTask.Application.Constants;
using SmartTask.Application.Dto;
using SmartTask.Application.Dto.Account;
using SmartTask.Application.Dto.Role;
using SmartTask.Application.Dto.Task;
using SmartTask.Application.Enums;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Query;
using SmartTask.Application.Wrappers;
using SmartTask.Domain.Constants;
using SmartTask.Domain.Entities;
using SmartTask.Identity.Models;
using SmartTask.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SmartTask.Domain.Constants.Permissions;

namespace SmartTask.Persistence.Repositories
{
    public class EntityMangerAsync : IEntityManagerAsync
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly ILogger<EntityMangerAsync> _logger;
        private readonly IDbConnection db;
        private readonly IAuditLogRepository _auditLogRepo;
        private readonly IUnitOfWork _unitOfWork;
        public EntityMangerAsync(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context, RoleManager<IdentityRole> roleManager, IAuthenticatedUserService authenticatedUserService, ILogger<EntityMangerAsync> logger, IDbConnection dbConnection, IAuditLogRepository auditLogRepo, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _roleManager = roleManager;
            _authenticatedUserService = authenticatedUserService;
            _logger = logger;
            db = dbConnection;
            _auditLogRepo = auditLogRepo;
            _unitOfWork = unitOfWork;
        }


        public async Task<Response<CompanyResponse>> RegisterCompanyAsync(CompanyRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (await _unitOfWork.Companies.CompanyExistsAsync(request.CompanyName))
                {
                    _logger.LogWarning("Company '{CompanyName}' already exists", request.CompanyName);
                    return ApplicationConstants.FailureMessage<CompanyResponse>(
                        null,
                        $"Company '{request.CompanyName}' already exists."
                    );
                }

                var company = new Domain.Entities.Company
                {
                    Name = request.CompanyName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    Description = request.Description,
                    Type = (Application.Enums.CompanyType)request.CompanyType,
                    Country = request.Country
                };

                await _unitOfWork.Companies.AddAsync(company);

                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    EmailConfirmed = true,
                    CreatedBy = request.Email,
                    CompanyName = request.CompanyName,
                    CompanyId = company.Id,
                    IsActive = true,
                    DateCreated = DateTime.UtcNow,
                    Type = request.CompanyType,
                    PhoneNumber = request.PhoneNumber
                };
                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return ApplicationConstants.FailureMessage<CompanyResponse>(
                        null,
                        $"Failed: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                    );
                }

                const string role = "CompanyAdmin";
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }

                await _userManager.AddToRoleAsync(user, role);

                // Commit changes using Unit of Work
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                var data = new CompanyResponse
                {
                    CompanyId = company.Id,
                    UserId = user.Id,
                    Email = request.Email
                };

                return ApplicationConstants.SuccessMessage(data, $"Company {request.CompanyName} registered successfully with user {request.Email}.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error registering company: {Message}", ex.Message);
                return ApplicationConstants.FailureMessage<CompanyResponse>(null, "An error occurred while registering the company.");
            }
        }



        public async Task<bool> UserExistsInCompanyAsync(string email)
        {
            var companyId = Guid.Parse(_authenticatedUserService.CompanyId!);
            return await _context.Users
                .AnyAsync(u =>
                    u.Email.ToLower() == email.ToLower()
                    && u.CompanyId == companyId
                );
        }

        public async Task<Response<UserResponseDto>> RegisterUserAsync(UserRequestDto request)
        {
            try
            {
                if (string.Equals(request.Role, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    return ApplicationConstants.FailureMessage<UserResponseDto>(null,
                        $"SuperAdmin role cannot be assigned to a '{request.Role}'.");
                }
                if (!Guid.TryParse(_authenticatedUserService.CompanyId, out var companyId))
                {
                    return ApplicationConstants.FailureMessage<UserResponseDto>(null,
                        "Unable to determine your company context.");
                }
                if (await UserExistsInCompanyAsync(request.Email))
                {
                    _logger.LogWarning(
                        "RegisterUserAsync: User '{Email}' already exists in company '{CompanyId}'",
                        request.Email, companyId);

                    return ApplicationConstants.FailureMessage<UserResponseDto>(null,
                        $"A user with email '{request.Email}' already exists in your company.");
                }
                var newUser = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    CompanyId = companyId,
                    CreatedBy = _authenticatedUserService.UserId
                };

                _logger.LogInformation(
                    "RegisterUserAsync: Creating user '{Email}' under company '{CompanyId}'",
                    request.Email, companyId);

                var result = await _userManager.CreateAsync(newUser, request.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    _logger.LogError(
                        "RegisterUserAsync: Failed to create user '{Email}': {Errors}",
                        request.Email, string.Join(", ", errors));

                    return ApplicationConstants.FailureMessage<UserResponseDto>(null,
                        string.Join(", ", errors));
                }
                if (!await _roleManager.RoleExistsAsync(request.Role))
                {
                    _logger.LogInformation("RegisterUserAsync: Creating role '{Role}'", request.Role);
                    await _roleManager.CreateAsync(new IdentityRole(request.Role));
                }

                await _userManager.AddToRoleAsync(newUser, request.Role);
                _logger.LogInformation(
                    "RegisterUserAsync: User '{Email}' assigned role '{Role}'",
                    request.Email, request.Role);

                var userResponse = new UserResponseDto
                {
                    Email = newUser.Email,
                    UserName = newUser.UserName,
                    Role = request.Role
                };

                return ApplicationConstants.SuccessMessage(userResponse,
                    "User registered successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user: {Message}", ex.Message);
                return ApplicationConstants.FailureMessage<UserResponseDto>(null,
                    "An error occurred while registering the user.");
            }
        }

        public async Task<Response<List<TaskDto>>> GetTaskByCompanyIdAsync(GetTaskByIdQuery request)
        {
            try
            {
                var tasks = await _unitOfWork.Tasks
                    .GetQueryable()
                    .Where(t => t.CompanyId == request.CompanyId && t.isActive)
                    .Select(t => new TaskDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        AssignedUserId = t.AssignedUserId,
                        DueDate = t.DueDate,
                        Priority = t.Priority.ToString(),
                        Status = t.Status.ToString(),
                        CreatedBy = t.CreatedBy,
                        CreatedAt = t.CreatedAt,
                        IsCompleted = t.IsCompleted
                    })
                    .ToListAsync();

                return ApplicationConstants.SuccessMessage(tasks, "Tasks retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks: {Message}", ex.Message);
                return ApplicationConstants.FailureMessage<List<TaskDto>>(null, "An error occurred while retrieving tasks.");
            }
        }

        public async Task<Response<List<TaskDto>>> GetTasksByAssignedUserAsync(GetTasksByAssignedUserQuery request)
        {
            try
            {

                var tasks = await _unitOfWork.Tasks.GetQueryable()
                    .Where(t => t.AssignedUserId == request.AssignedUserId && t.isActive)
                    .Select(t => new TaskDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        AssignedUserId = t.AssignedUserId,
                        DueDate = t.DueDate,
                        Priority = t.Priority.ToString(),
                        Status = t.Status.ToString(),
                        CreatedBy = t.CreatedBy,
                        CreatedAt = t.CreatedAt,
                        IsCompleted = t.IsCompleted
                    })
                    .ToListAsync();
                return ApplicationConstants.SuccessMessage(tasks, "Tasks retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks by assigned user: {Message}", ex.Message);
                return ApplicationConstants.FailureMessage<List<TaskDto>>(null, "An error occurred while retrieving tasks by assigned user.");
            }
        }

        public async Task<Response<List<TaskCompletionStatus>>> GetTasksCompletedPerWeekAsync()
        {
            try
            {
                var companyId = Guid.Parse(_authenticatedUserService.CompanyId);
                if (companyId == null) return ApplicationConstants.FailureMessage<List<TaskCompletionStatus>>(null, "Invalid company context.");

                var rows = await db.QueryAsync<TaskCompletionStatus>(
                    "dbo.sp_GetTasksWeeklyStats",
                    new { CompanyId = companyId },
                    commandType: CommandType.StoredProcedure);

                var list = rows.AsList();
                if (!list.Any())
                    return ApplicationConstants.FailureMessage<List<TaskCompletionStatus>>(null, "No weekly task stats found.");

                return ApplicationConstants.SuccessMessage(list, "Weekly task analytics retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving weekly task stats");
                return ApplicationConstants.FailureMessage<List<TaskCompletionStatus>>(null,
                    "An error occurred while retrieving weekly task analytics.");
            }
        }
        public async Task<Response<List<TaskCompletionStatus>>> GetFilteredTasksAsync(WeeklyStatsFilter request)
        {
            try
            {
                var companyId = Guid.Parse(_authenticatedUserService.CompanyId);
                if (companyId == Guid.Empty)
                    return ApplicationConstants.FailureMessage<List<TaskCompletionStatus>>(null, "Invalid company context.");

                var rows = await db.QueryAsync<TaskCompletionStatus>(
                    "dbo.sp_GetTasksWeeklyStats",
                    new { CompanyId = companyId },
                    commandType: CommandType.StoredProcedure);

                var list = rows.AsList();

                if (!string.IsNullOrWhiteSpace(request.Week))
                    list = list.Where(x => x.Week == request.Week).ToList();

                if (request.MinCompletedTasks.HasValue)
                    list = list.Where(x => x.CompletedTasks >= request.MinCompletedTasks.Value).ToList();

                if (request.Status.HasValue)
                    list = list.Where(x => x.Status == request.Status.Value).ToList();

                if (!list.Any())
                    return ApplicationConstants.FailureMessage<List<TaskCompletionStatus>>(null, "No stats match the filter.");

                return ApplicationConstants.SuccessMessage(list, "Filtered task stats retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving filtered weekly task stats");
                return ApplicationConstants.FailureMessage<List<TaskCompletionStatus>>(null,
                    "An error occurred while retrieving weekly task analytics.");
            }
        }

        public async Task<Response<List<OverdueTaskStatsDto>>> GetOverdueTasksAsync()
        {
            try
            {
                var companyId = Guid.Parse(_authenticatedUserService.CompanyId);
                if (companyId == Guid.Empty)
                    return ApplicationConstants.FailureMessage<List<OverdueTaskStatsDto>>(null, "Invalid company context.");

                var rows = await db.QueryAsync<OverdueTaskStatsDto>(
                    "dbo.sp_GetOverdueTasksStats",
                    new { CompanyId = companyId },
                    commandType: CommandType.StoredProcedure);

                var list = rows.AsList();

                if (!list.Any())
                    return ApplicationConstants.FailureMessage<List<OverdueTaskStatsDto>>(null, "No overdue task stats found.");

                return ApplicationConstants.SuccessMessage(list, "Overdue task analytics retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving overdue task stats");
                return ApplicationConstants.FailureMessage<List<OverdueTaskStatsDto>>(null,
                    "An error occurred while retrieving overdue task analytics.");
            }
        }
        public async Task<Response<List<OverdueTaskStatsDto>>> GetFilteredOverdueTasksAsync(FilteredOverdueTask request)
        {
            try
            {
                if (request == null)
                    return ApplicationConstants.FailureMessage<List<OverdueTaskStatsDto>>(null, "Invalid request.");

                var companyId = Guid.Parse(_authenticatedUserService.CompanyId);
                if (companyId == Guid.Empty)
                    return ApplicationConstants.FailureMessage<List<OverdueTaskStatsDto>>(null, "Invalid company.");

                var rows = await db.QueryAsync<OverdueTaskStatsDto>(
                    "dbo.sp_GetOverdueTasksStats",
                    new { CompanyId = companyId },
                    commandType: CommandType.StoredProcedure);

                if (rows == null || !rows.Any())
                    return ApplicationConstants.FailureMessage<List<OverdueTaskStatsDto>>(null, "No overdue task stats found.");

                var filtered = rows.Where(r =>
                    (string.IsNullOrEmpty(request.Week) || string.Equals(r?.Week ?? "", request.Week, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrEmpty(request.UserEmail) || string.Equals(r?.AssignedUserEmail ?? "", request.UserEmail, StringComparison.OrdinalIgnoreCase)) &&
                    (!request.MinOverDueCount.HasValue || (r?.OverdueCount ?? 0) >= request.MinOverDueCount.Value)
                ).ToList();

                return ApplicationConstants.SuccessMessage(filtered, "Filtered overdue tasks retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetFilteredOverdueTasksAsync");
                return ApplicationConstants.FailureMessage<List<OverdueTaskStatsDto>>(null, "An error occurred while filtering overdue tasks.");
            }
        }
        public async Task<Response<List<CompanyTypeDto>>> GetAllCompanyTypeAsync()
        {
            try
            {
                var companyTypes = Enum.GetValues(typeof(Application.Enums.CompanyType))
                    .Cast<Application.Enums.CompanyType>()
                    .Select(ct => new CompanyTypeDto
                    {
                        Id = (int)ct,
                        Name = ct.ToString()
                    })
                    .ToList();
                if (!companyTypes.Any())
                {
                    return ApplicationConstants.FailureMessage<List<CompanyTypeDto>>(null, "No company types found.");
                }
                return ApplicationConstants.SuccessMessage(companyTypes, "Company types retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving company types: {Message}", ex.Message);
                return ApplicationConstants.FailureMessage<List<CompanyTypeDto>>(null, "An error occurred while retrieving company types.");
            }

        }
        public async Task<Response<List<string>>> AddPermissionAsync(PermissionDto request)
        {
            try
            {
                // Validate role
                var role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == request.RoleName);
                if (role == null)
                {
                    return ApplicationConstants.NotFoundMessage<List<string>>(null, $"Role '{request.RoleName}' not found.");
                }

                // Fetch requested permissions from IdentityContext
                var permissions = await _context.Permission
                    .Where(p => request.Permissions.Contains(p.Name))
                    .ToListAsync();

                var addedPermissions = new List<string>();

                foreach (var permission in permissions)
                {
                    var exists = await _context.RolePermission
                        .AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);

                    if (!exists)
                    {
                        _context.RolePermission.Add(new RolePermission
                        {
                            RoleId = role.Id,
                            PermissionId = permission.Id
                        });

                        addedPermissions.Add(permission.Name);
                    }
                }

                if (addedPermissions.Any())
                {
                    await _context.SaveChangesAsync();
                    return ApplicationConstants.SuccessMessage(addedPermissions, $"Added {addedPermissions.Count} permission(s) to role '{role.Name}'.");
                }

                return ApplicationConstants.FailureMessage(addedPermissions, $"No new permissions were added to role '{role.Name}' (all already assigned).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding permissions to the role.");
                return ApplicationConstants.FailureMessage<List<string>>(null, $"An error occurred while adding permissions to role '{request.RoleName}'.");
            }
        }

    }
}



