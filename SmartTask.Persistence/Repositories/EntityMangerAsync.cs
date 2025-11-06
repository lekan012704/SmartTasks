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
using SmartTask.Application.Dto.Role;
using SmartTask.Application.Enums;
using SmartTask.Application.Features.Orders.Commands;
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
        public async Task<Guid> CreateOrderAsync(CreateOrderCommand request,CancellationToken cancellationToken)
        {
            
        }

    }

}




