using Azure.Core;
using Dapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SmartTask.Application.Command;
using SmartTask.Application.Command.Task;
using SmartTask.Application.Constants;
using SmartTask.Application.Dto;
using SmartTask.Application.Dto.Account;
using SmartTask.Application.Dto.Project;
using SmartTask.Application.Dto.Role;
using SmartTask.Application.Dto.Task;
using SmartTask.Application.Enums;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Query;
using SmartTask.Application.Wrappers;
using SmartTask.Domain.Constants;
using SmartTask.Domain.Entities;
using SmartTask.Domain.Models;
using SmartTask.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
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

                // ====================================================================
                // START FIX: Look up Company Name from CompanyId
                // ====================================================================

                // Assuming your DbContext (_context) has a DbSet for Companies (e.g., _context.Companies)
                // And your Company entity has a 'Name' property. Adjust if your entity is named differently.
                var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
                if (company == null)
                {
                    _logger.LogError("RegisterUserAsync: Company not found for ID '{CompanyId}'", companyId);
                    return ApplicationConstants.FailureMessage<UserResponseDto>(null, "Unable to find your company details.");
                }
                var companyName = company.Name; // Get the company name as a string

                // ====================================================================
                // END FIX
                // ====================================================================

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
                    UserName = request.UserName, // <-- Use request.UserName
                    Email = request.Email,
                    CompanyId = companyId,
                    CompanyName = companyName,        // <-- FIX 1: Set the CompanyName
                    FullName = request.FullName,      // <-- FIX 2: Set the FullName
                    PhoneNumber = request.PhoneNumber,  // <-- FIX 3: Set the PhoneNumber
                    CreatedBy = _authenticatedUserService.UserId,
                    IsActive = true,
                    DateCreated = request.DateCreated
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
                    FullName = newUser.FullName,
                    PhoneNumber = newUser.PhoneNumber,
                    Role = request.Role,
                    IsActive = request.IsActive,
                    DateCreated = request.DateCreated
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

        public async Task<Response<List<TaskDto>>> GetTaskByCompanyIdAsync(GetTaskByComapanyIdQuery request)
        {
            try
            {
                // --- UPDATED QUERY ---
                var tasks = await _unitOfWork.Tasks
                    .GetQueryable()
                    // 1. FILTER BY COMPANYID ONLY. Do not filter by isActive or isDeleted.
                    .Where(t => t.CompanyId == request.CompanyId)
                    // 2. Use GroupJoin (LEFT JOIN) to safely join with Users
                    .GroupJoin(
                        _context.Users, // Your user table
                        task => task.AssignedUserId,
                        user => user.Id,
                        (task, usersGroup) => new { Task = task, Users = usersGroup }
                    )
                    .SelectMany(
                        joined => joined.Users.DefaultIfEmpty(), // Makes it a LEFT JOIN
                        (joined, user) => new TaskDto // 3. Select from the joined object
                        {
                            Id = joined.Task.Id,
                            Title = joined.Task.Title,
                            Description = joined.Task.Description,
                            AssignedUserId = joined.Task.AssignedUserId,
                            AssignedUserName = user != null ? user.UserName : "Unassigned",
                            DueDate = joined.Task.DueDate,
                            Priority = joined.Task.Priority.ToString(),
                            Status = joined.Task.Status.ToString(),
                            CreatedBy = joined.Task.CreatedBy,
                            CreatedAt = joined.Task.CreatedAt,
                            IsCompleted = joined.Task.IsCompleted,
                            IsActive = joined.Task.isActive,
                            IsDeleted = joined.Task.IsDeleted // <-- This will now be sent
                        })
                    .ToListAsync();

                if (!tasks.Any())
                {
                    return ApplicationConstants.SuccessMessage(new List<TaskDto>(), "No tasks found for this company.");
                }

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
        public async Task<Response<List<UserDto>>> GetUsersByCompanyAsync(GetUsersByCompany request)
        {
            var users = await _userManager.Users
                .Where(u => u.CompanyId == request.CompanyId)
                .ToListAsync();

            var result = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? "No Email",
                    UserName = user.UserName ?? "No UserName",
                    Role = roles.FirstOrDefault() ?? "No Role",
                    IsActive = user.IsActive,
                    DateCreated = user.DateCreated,
                    PhoneNumber = user.PhoneNumber ?? "No Phone Number",
                    CreatedBy = _authenticatedUserService.UserName,
                    FullName = user.FullName ?? "No FullName"
                });
            }

            if (!result.Any())
            {
                return ApplicationConstants.FailureMessage<List<UserDto>>(null, "No users found for the specified company.");
            }

            return ApplicationConstants.SuccessMessage(result, "Users retrieved successfully.");
        }
        public async Task<Response<List<CompletedTaskDto>>> GetTasksCompletedAsync()
        {
            try
            {
                if (!Guid.TryParse(_authenticatedUserService.CompanyId, out var companyId))
                {
                    return ApplicationConstants.FailureMessage<List<CompletedTaskDto>>(null, "Invalid company context.");
                }

                var parameters = new DynamicParameters();
                parameters.Add("CompanyId", companyId, DbType.Guid);

                var tasks = await db.QueryAsync<CompletedTaskDto>(
                    "dbo.sp_GetCompletedTasks",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                var taskList = tasks.AsList();
                if (!taskList.Any())
                {
                    return ApplicationConstants.FailureMessage<List<CompletedTaskDto>>(null, "No completed tasks found.");
                }

                return ApplicationConstants.SuccessMessage(taskList, "Completed tasks retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving completed tasks");
                return ApplicationConstants.FailureMessage<List<CompletedTaskDto>>(null,
                    "An error occurred while retrieving completed tasks.");
            }
        }
        public async Task<Response<bool>> CompleteTaskAsync(Guid taskId)
        {
            try
            {
                if (!Guid.TryParse(_authenticatedUserService.CompanyId, out var companyId) || companyId == Guid.Empty)
                {
                    _logger.LogWarning("CompleteTaskCommand: Invalid CompanyId '{CompanyId}' in user context.", _authenticatedUserService.CompanyId);
                    return ApplicationConstants.FailureMessage<bool>(false, "Invalid company context.");
                }

                var task = await _unitOfWork.Tasks
                    .FirstOrDefaultAsync(t => t.Id == taskId && t.CompanyId == companyId && !t.IsDeleted);

                if (task == null)
                {
                    _logger.LogWarning("CompleteTaskAsync: Task not found or access denied (ID: {TaskId}, Company: {CompanyId})", taskId, companyId);
                    return ApplicationConstants.FailureMessage<bool>(false, "Task not found or you do not have permission to access it.");
                }

                // Check if the task is already completed to avoid unnecessary updates/logs
                if (task.IsCompleted)
                {
                    _logger.LogInformation("CompleteTaskAsync: Task already completed (ID: {TaskId})", taskId);
                    // Return success, as the desired state is already achieved
                    return ApplicationConstants.SuccessMessage(true, "Task is already marked as complete.");
                }

                // --- Prepare Audit Data (Before Changes) ---
                var oldValues = new Dictionary<string, object>
            {
                { nameof(task.Status), task.Status.ToString() }, // Record previous status
                { nameof(task.IsCompleted), task.IsCompleted }    // Record previous completion state
            };

                // --- Apply Changes ---
                task.Status = TaskStatuses.Completed; // Set status to Completed
                task.IsCompleted = true;              // Explicitly set the IsCompleted flag
                task.UpdatedAt = DateTime.UtcNow;     // Update the modification timestamp

                // --- Save Changes to Database ---
                await _unitOfWork.SaveChangesAsync();

                // --- Create Audit Log Entry (After Successful Save) ---
                var newValues = new Dictionary<string, object>
            {
                { nameof(task.Status), task.Status.ToString() },
                { nameof(task.IsCompleted), task.IsCompleted }
            };

                // Assuming TaskActivity has these properties
                var auditLog = new TaskActivity
                {
                    TaskId = task.Id,
                    Action = "TaskCompleted", // Use a specific action name
                    PerformedBy = _authenticatedUserService.UserId,
                    OldValue = JsonConvert.SerializeObject(oldValues), // Serialize dictionaries
                    NewValue = JsonConvert.SerializeObject(newValues),
                    Timestamp = DateTime.UtcNow // Use consistent timestamp
                };
                await _auditLogRepo.InsertAsync(auditLog);
                // Depending on your UnitOfWork/Repository pattern, you might need
                // another _unitOfWork.SaveChangesAsync() here if the audit repo is separate.
                // If the audit repo uses the same DbContext and SaveChangesAsync saves everything,
                // then a second call might not be needed. Check your implementation.
                await _unitOfWork.SaveChangesAsync(); // Save the audit log if needed


                _logger.LogInformation("Task completed successfully (ID: {TaskId}) by User: {UserId}", taskId, _authenticatedUserService.UserId);
                return ApplicationConstants.SuccessMessage(true, "Task marked as complete successfully.");
            }
            catch (DbUpdateConcurrencyException ex) // Handle potential race conditions
            {
                _logger.LogWarning(ex, "Concurrency conflict detected while completing TaskId: {TaskId}", taskId);
                // Provide a user-friendly message for concurrency issues
                return ApplicationConstants.FailureMessage<bool>(false, "Could not complete the task because it was modified by someone else simultaneously. Please refresh and try again.");
            }
            catch (Exception ex) // Catch broader exceptions
            {
                _logger.LogError(ex, "Error occurred in CompleteTaskAsync for TaskId: {TaskId}", taskId);
                // Return a generic failure message for unexpected errors
                return ApplicationConstants.FailureMessage<bool>(false, "An error occurred while marking the task as complete.");
            }
        }
        public async Task<TaskDto> GetTasksByIdAsync(Guid taskId)
        {
            _logger.LogInformation("Attempting to retrieve task with ID: {TaskId}", taskId);
            try
            {
                // Assuming _context.TaskItems and _context.Users are your DbSets
                // Using Left Join to handle cases where AssignedUserId might be null or user doesn't exist
                var taskDto = await _context.TaskItem
                    .Where(t => t.Id == taskId && !t.IsDeleted) // Also check IsDeleted for consistency
                    .GroupJoin( // Use GroupJoin for a LEFT JOIN equivalent
                        _context.Users,
                        task => task.AssignedUserId,
                        user => user.Id,
                        (task, usersGroup) => new { task, usersGroup }
                    )
                  .SelectMany(
    x => x.usersGroup.DefaultIfEmpty(),
    (x, user) => new TaskDto
    {
        Id = x.task.Id,
        Title = x.task.Title,
        Description = x.task.Description,
        AssignedUserName = user != null ? user.UserName : "Unassigned",
        DueDate = x.task.DueDate,
        Priority = x.task.Priority.ToString(),
        Status = x.task.Status.ToString(),
        CreatedBy = x.task.CreatedBy,
        CreatedAt = x.task.CreatedAt,
        IsCompleted = x.task.IsCompleted,
        IsActive = x.task.isActive,
        IsDeleted = x.task.IsDeleted
    })
    .FirstOrDefaultAsync();

                if (taskDto == null)
                {
                    _logger.LogWarning("Task with ID {TaskId} not found or is deleted.", taskId);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved task with ID: {TaskId}", taskId);
                }

                return taskDto;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task with ID {TaskId} in EntityManagerService", taskId);
                return null; // Return null on error as per handler expectation
            }
        }
        public async Task<Response<List<string>>> AddPermissionsToRoleAsync(PermissionDto request)
        {
            try
            {
                var role = await _roleManager.FindByNameAsync(request.RoleName);
                if (role == null)
                {
                    return ApplicationConstants.NotFoundMessage<List<string>>(null, $"Role '{request.RoleName}' not found.");
                }


                var permissionsInDb = await _context.Set<Permission>()
                    .Where(p => request.Permissions.Contains(p.Name))
                    .ToListAsync();

                if (!permissionsInDb.Any())
                {
                    return ApplicationConstants.FailureMessage<List<string>>(null, $"None of the specified permissions were found in the database.");
                }

                var permissionNamesInDb = permissionsInDb.Select(p => p.Name).ToList();
                var invalidPermissions = request.Permissions.Except(permissionNamesInDb).ToList();

                if (invalidPermissions.Any())
                {
                    _logger.LogWarning("Invalid permission names provided for role {RoleName}: {InvalidPermissions}", request.RoleName, string.Join(", ", invalidPermissions));

                }


                var addedPermissions = new List<string>();
                var existingPermissionIds = await _context.Set<RolePermission>()
                                            .Where(rp => rp.RoleId == role.Id)
                                            .Select(rp => rp.PermissionId)
                                            .ToListAsync();

                foreach (var permission in permissionsInDb)
                {
                    if (!existingPermissionIds.Contains(permission.Id))
                    {
                        _context.Set<RolePermission>().Add(new RolePermission
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
                    string message = $"Added {addedPermissions.Count} permission(s) to role '{role.Name}'.";
                    if (invalidPermissions.Any())
                    {
                        message += $" Ignored invalid permissions: {string.Join(", ", invalidPermissions)}.";
                    }
                    return ApplicationConstants.SuccessMessage(addedPermissions, message);
                }

                string failureMsg = $"No new permissions were added to role '{role.Name}'.";
                if (invalidPermissions.Any())
                {
                    failureMsg += $" Invalid permissions specified: {string.Join(", ", invalidPermissions)}.";
                }
                else
                {
                    failureMsg += " All specified permissions were already assigned or invalid.";
                }
                return ApplicationConstants.SuccessMessage(new List<string>(), failureMsg);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding permissions to the role {RoleName}.", request.RoleName);
                return ApplicationConstants.FailureMessage<List<string>>(null, $"An internal error occurred while adding permissions to role '{request.RoleName}'.");
            }
        }

        public async Task<Response<string>> UpdateUserAsync(string Id, UpdateUserRequestDto requestDto)
        {
            try

            {
                var user = await _userManager.FindByIdAsync(Id);
                if (user == null)
                {
                    return new Response<string>($"User with ID {(Id)} not found.");
                }
                user.FullName = requestDto.FullName;
                user.PhoneNumber = requestDto.PhoneNumber;
                user.Email = requestDto.Email;
                user.UserName = requestDto.UserName;
                user.IsActive = true;
                user.DateCreated = requestDto.DateCreated;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    return new Response<string>("Failed to update user");
                }
                if (!string.IsNullOrWhiteSpace(requestDto.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var passwordResult = await _userManager.ResetPasswordAsync(user, token, requestDto.Password);
                    if (!passwordResult.Succeeded)
                    {
                        return new Response<string>("Failed to update password.");
                    }
                }

                if (!string.IsNullOrWhiteSpace(requestDto.Role))
                {

                    if (!await _roleManager.RoleExistsAsync(requestDto.Role))
                    {
                        return new Response<string>($"Role '{requestDto.Role}' does not exist.");
                    }

                    var currentRoles = await _userManager.GetRolesAsync(user);
                    // Remove old roles
                    var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removeRolesResult.Succeeded)
                    {
                        return new Response<string>("Failed to remove existing roles.");
                    }

                    var addRoleResult = await _userManager.AddToRoleAsync(user, requestDto.Role);
                    if (!addRoleResult.Succeeded)
                    {
                        return new Response<string>("Failed to add new role.");
                    }
                }

                return new Response<string>(user.Id, "User updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing User to the  {UserName}.", requestDto.UserName);
                return new Response<string>($"An internal error occurred while adding permissions to role '{requestDto.UserName}'.");
            }
        }
        public async Task<Response<string>> DeleteUserAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new Response<string>($"User with ID {userId} not found.");
                }
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("SuperAdmin") || roles.Contains("CompanyAdmin"))
                {
                    return new Response<string>("A SuperAdmin or CompanyAdmin account cannot be deleted.");
                }
                var deleteResult = await _userManager.DeleteAsync(user);
                if (!deleteResult.Succeeded)
                {
                    return new Response<string>("Failed to delete user.");
                }
                return new Response<string>(userId, "User deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting user {UserId}.", userId);
                return new Response<string>($"An internal error occurred while deleting user '{userId}'.");
            }
        }
        public async Task<Response<string>> ActivateUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new Response<string>("User not found.");
            }

            if (user.IsActive == true)
            {
                return new Response<string>("User is already active.");
            }
            user.IsActive = true;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return new Response<string>("Failed to update user.");
            }

            return new Response<string>("User activated successfully.");
        }
        public async Task<Response<string>> DeactivateUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new Response<string>("User not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("SuperAdmin") || roles.Contains("CompanyAdmin"))
            {
                return new Response<string>("A SuperAdmin or CompanyAdmin account cannot be deactivated.");
            }


            if (user.IsActive == false)
            {
                return new Response<string>("User is already inactive.");
            }

            user.IsActive = false;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {

                return new Response<string>("Failed to update user.");
            }

            return new Response<string>("User deactivated successfully.");
        }
        public async Task<Response<ProjectDto>> GetProjectByIdAsync(Guid projectId)
        {
            try
            {
                var project = await _context.Project
               .Include(p => p.Members)
               .Include(p => p.Tasks)
               .Include(p => p.Sprints)
               .FirstOrDefaultAsync(p => p.ProjectId == projectId && !p.IsDeleted);

                if (project == null) return null;
                var result = new ProjectDto
                {
                    ProjectName = project.ProjectName,
                    Status = project.Status.ToString(),
                    Visibility = project.Visibility.ToString(),
                    CompanyId = project.CompanyId,
                    ProjectLeadId = project.ProjectLeadId,
                    Slug = project.Slug,
                    Description = project.Description,
                    StartDate = project.StartDate,
                    DueDate = project.DueDate,
                    TotalTasks = project.TotalTasks,
                    OpenTasks = project.OpenTasks,
                    IsDeleted = project.IsDeleted,
                    IsArchived = project.IsArchived,
                    CreatedAt = project.CreatedAt,
                    CreatedBy = project.CreatedBy,
                    UpdatedAt = project.UpdatedAt,
                    UpdatedBy = project.UpdatedBy
                };
                return Response<ProjectDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting project {ProjectId}.", projectId);
                return Response<ProjectDto>.Failure($"An internal error occurred while deleting user '{projectId}'.");
            }

        }
        public async Task<Response<List<ProjectDto>>> GetProjectByCompanyIdAsync()
        {
            try
            {
                if (!Guid.TryParse(_authenticatedUserService.CompanyId, out var companyId))
                {
                    return Response<List<ProjectDto>>.Failure(
                        "Unable to determine your company context.");
                }
                var projects = await _context.Project
                    .Where(p => p.CompanyId == companyId)
                    .GroupJoin(
                        _context.Users,
                        project => project.ProjectLeadId,
                        user => user.Id,
                        (project, usersGroup) => new { Project = project, Users = usersGroup }
                    )
                    .SelectMany(
                        joined => joined.Users.DefaultIfEmpty(), 
                        (joined, user) => new ProjectDto
                        {
                            ProjectId = joined.Project.ProjectId,
                            ProjectName = joined.Project.ProjectName,
                            Status = joined.Project.Status.ToString(),
                            Visibility = joined.Project.Visibility.ToString(),
                            ProjectLeadId = joined.Project.ProjectLeadId,
                            Description = joined.Project.Description,
                            StartDate = joined.Project.StartDate,
                            DueDate = joined.Project.DueDate,
                            Members = joined.Project.Members.Select(m => new ProjectMemberDto
                            {
                                UserId = m.UserId ?? string.Empty,
                                Role = m.Role ?? "Employee",
                                DateJoined = m.DateJoined
                            }).ToList()
                        }
                    )
                    .ToListAsync();

                if (!projects.Any())
                {
                    return Response<List<ProjectDto>>.Success(new List<ProjectDto>(), "No projects found for this company.");
                }

                return Response<List<ProjectDto>>.Success(projects, "Projects retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving projects: {Message}", ex.Message);
                return Response<List<ProjectDto>>.Failure($"An error occurred: {ex.Message}");
            }
        }


        public async Task<Response<CreateProjectResponse>> CreateProjectAsync(CreateProjectRequest request)
        {
            try
            {
                if (request == null)
                    return new Response<CreateProjectResponse>("Request cannot be null.");

                if (_authenticatedUserService == null)
                    return new Response<CreateProjectResponse>("User service not available.");

                var project = new Domain.Entities.Project
                {
                    ProjectId = Guid.NewGuid(),
                    ProjectName = request.ProjectName,
                    Description = request.Description,
                    CompanyId = request.CompanyId,
                    ProjectLeadId = request.ProjectLeadId,
                    Slug = request.Slug,
                    Status = Enum.TryParse<ProjectStatus>(request.Status, true, out var status) ? status : ProjectStatus.Planning,
                    Visibility = Enum.TryParse<ProjectVisibility>(request.Visibility, true, out var visibility) ? visibility : ProjectVisibility.Private,
                    StartDate = request.StartDate,
                    DueDate = request.DueDate,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = _authenticatedUserService.UserId,
                    Members = new List<ProjectMember>()
                };

                if (request.Members != null && request.Members.Any())
                {
                    foreach (var member in request.Members)
                    {
                        if (string.IsNullOrEmpty(member.UserId)) continue;

                        project.Members.Add(new ProjectMember
                        {
                            UserId = member.UserId,
                            Role = member.Role,
                            DateJoined = DateTime.UtcNow
                        });
                    }
                }

                if (_unitOfWork?.Project == null)
                    return new Response<CreateProjectResponse>("Unit of Work or Project Repository is null.");

                await _unitOfWork.Project.AddAsync(project);
                await _unitOfWork.SaveChangesAsync();

                return new Response<CreateProjectResponse>(new CreateProjectResponse
                {
                    ProjectId = project.ProjectId,
                    ProjectName = project.ProjectName,
                    Status = project.Status.ToString(),
                    Visibility = project.Visibility.ToString(),
                    CreatedAt = project.CreatedAt,
                    CreatedBy = project.CreatedBy,
                    IsSuccess = true,
                    Message = "Project created successfully."
                });
            }
            catch (Exception ex)
            {
                return new Response<CreateProjectResponse>($"Error creating project: {ex.Message}");
            }
        }


    }

}




