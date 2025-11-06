using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartTask.Application.Command;
using SmartTask.Application.Constants;
using SmartTask.Application.Dto.Account;
using SmartTask.Application.Dto.Role;
using SmartTask.Application.Interfaces;
using SmartTask.Application.Wrappers;
using SmartTask.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Identity.Services
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RoleService> _logger;
        public RoleService(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ILogger<RoleService> logger)
        {
            // Constructor logic can be added here if needed
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }
        public async Task<bool> RoleExistsAsync(string roleName)
        {
            return await _roleManager.RoleExistsAsync(roleName);
        }
        public async Task<Response<string>> CreateRoleAsync(CreateRoleModel request)
        {
            try
            {
                if (await RoleExistsAsync(request.RoleName))
                    return ApplicationConstants.AlreadyExistMessage($"Role '{request.RoleName}' already exists.");

                var identityRole = new IdentityRole(request.RoleName);
                var result = await _roleManager.CreateAsync(identityRole);

                if (!result.Succeeded)
                    return ApplicationConstants.FailureMessage($"Failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

                // Add claims if provided
                if (request.Claims != null && request.Claims.Any())
                {
                    foreach (var claim in request.Claims)
                    {
                        await _roleManager.AddClaimAsync(identityRole, new System.Security.Claims.Claim("permission", claim));
                    }
                }
                return ApplicationConstants.SuccessMessage("Role created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role");
                return ApplicationConstants.FailureMessage("An error occurred while creating the role.");
            }
        }


        public async Task<Response<string>> AssignRoleToUserAsync(string userId, string roleName)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return ApplicationConstants.NotFoundMessage("User not found.");

                if (!await RoleExistsAsync(roleName))
                    return ApplicationConstants.NotFoundMessage($"Role '{roleName}' does not exist.");

                var result = await _userManager.AddToRoleAsync(user, roleName);

                if (!result.Succeeded)
                    return ApplicationConstants.FailureMessage($"Failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

                return ApplicationConstants.SuccessMessage("Role assigned successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role to user");
                return ApplicationConstants.FailureMessage("An error occurred while assigning the role to the user.");
            }
        }

        public async Task<Response<List<RoleDto>>> GetAllRolesAsync()
        {
            try
            {
                var roles = _roleManager.Roles.ToList();
                var roleDtos = new List<RoleDto>();

                foreach (var role in roles)
                {
                    var claims = await _roleManager.GetClaimsAsync(role);

                    roleDtos.Add(new RoleDto
                    {
                        Id = role.Id,
                        RoleName = role.Name,
                        IsActive = true,
                        Claims = claims
                            .Where(c => c.Type == "permission")
                            .Select(c => c.Value)
                            .ToList()
                    });
                }

                if (!roleDtos.Any())
                    return ApplicationConstants.NotFoundMessage(new List<RoleDto>(), "No roles found.");

                return ApplicationConstants.SuccessMessage(roleDtos, "Roles fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching roles");
                return ApplicationConstants.FailureMessage<List<RoleDto>>(null, "An error occurred while fetching roles.");
            }
        }


        public async Task<Response<RoleDto?>> GetRoleByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return ApplicationConstants.FailureMessage<RoleDto?>(null, "Role ID is required.");

                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                    return ApplicationConstants.NotFoundMessage<RoleDto?>(null, $"Role with ID '{id}' was not found.");

                var claims = await _roleManager.GetClaimsAsync(role);

                var roleDto = new RoleDto
                {
                    Id = role.Id,
                    RoleName = role.Name,
                    IsActive = true,
                    Claims = claims
                        .Where(c => c.Type == "permission")
                        .Select(c => c.Value)
                        .ToList()
                };

                return ApplicationConstants.SuccessMessage<RoleDto?>(roleDto, "Role fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching role by ID");
                return ApplicationConstants.FailureMessage<RoleDto?>(null, "An error occurred while fetching the role.");
            }
        }


        public async Task<Response<string>> UpdateRoleAsync(UpdateRoleCommand request)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(request.RoleId);
                if (role == null)
                    return ApplicationConstants.NotFoundMessage("Role not found.");

                role.Name = request.RoleName;
                var result = await _roleManager.UpdateAsync(role);
                if (!result.Succeeded)
                    return ApplicationConstants.FailureMessage($"Update failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");

                var existingClaims = await _roleManager.GetClaimsAsync(role);
                foreach (var claim in existingClaims)
                {
                    await _roleManager.RemoveClaimAsync(role, claim);
                }

                foreach (var claimValue in request.Claims.Distinct())
                {
                    await _roleManager.AddClaimAsync(role, new Claim("Permission", claimValue));
                }

                return ApplicationConstants.SuccessMessage("Role updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role");
                return ApplicationConstants.FailureMessage("An error occurred while updating the role.");
            }
        }

        public async Task<Response<string>> DeleteRoleAsync(string roleId)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                    return ApplicationConstants.NotFoundMessage("Role not found.");

                var result = await _roleManager.DeleteAsync(role);
                return result.Succeeded
                    ? ApplicationConstants.SuccessMessage("Role deleted successfully.")
                    : ApplicationConstants.FailureMessage($"Delete failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role");
                return ApplicationConstants.FailureMessage("An error occurred while deleting the role.");
            }
        }

        public async Task<Response<string>> RemoveRoleFromUserAsync(RemoveUserRoleCommand request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                    return ApplicationConstants.NotFoundMessage("User not found.");

                if (!await _userManager.IsInRoleAsync(user, request.RoleName))
                    return ApplicationConstants.FailureMessage($"User is not in role '{request.RoleName}'.");

                var result = await _userManager.RemoveFromRoleAsync(user, request.RoleName);
                return result.Succeeded
                    ? ApplicationConstants.SuccessMessage($"Role '{request.RoleName}' removed from user.")
                    : ApplicationConstants.FailureMessage($"Failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role from user");
                return ApplicationConstants.FailureMessage("An error occurred while removing the role from the user.");
            }
        }

        public async Task<Response<List<string>>> GetUsersInRoleAsync(string roleName)
        {
            try
            {
                var users = await _userManager.GetUsersInRoleAsync(roleName);

                if (users == null || !users.Any())
                    return ApplicationConstants.NotFoundMessage(new List<string>(), $"No users found in role '{roleName}'.");

                var usernames = users.Select(u => u.UserName).ToList();
                return ApplicationConstants.SuccessMessage(usernames, $"Users in role '{roleName}' retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users in role");
                return ApplicationConstants.FailureMessage<List<string>>(null, "An error occurred while fetching users in the role.");
            }
        }

        public async Task<Response<string>> AddClaimsToRoleAsync(string roleId, List<string> claims)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                    return ApplicationConstants.NotFoundMessage("Role not found.");

                var existingClaims = await _roleManager.GetClaimsAsync(role);
                foreach (var claimValue in claims.Distinct())
                {
                    if (!existingClaims.Any(c => c.Type == "Permission" && c.Value == claimValue))
                    {
                        await _roleManager.AddClaimAsync(role, new Claim("Permission", claimValue));
                    }
                }

                return ApplicationConstants.SuccessMessage("Claims added successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding claims to role");
                return ApplicationConstants.FailureMessage("An error occurred while adding claims to the role.");


            }
        }
        public async Task<Response<List<string>>> AddPermissionUserAsync(AssignUserPermissionsDto request)
        {
            // Define the specific claim type used for application permissions.
            const string PermissionClaimType = "permission";

          
            if (request == null || string.IsNullOrWhiteSpace(request.UserId) || request.Permissions == null)
            {
                return ApplicationConstants.FailureMessage<List<string>>(null, "Invalid request: UserId and Permissions list are required.");
            }

            try
            {
               
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    
                    return ApplicationConstants.NotFoundMessage<List<string>>(null, $"User with ID '{request.UserId}' not found.");
                }

          

               
                var currentClaims = await _userManager.GetClaimsAsync(user);
               
                var claimsToRemove = currentClaims.Where(c => c.Type == PermissionClaimType).ToList();

              
                if (claimsToRemove.Any())
                {
                    var removeResult = await _userManager.RemoveClaimsAsync(user, claimsToRemove);
                    if (!removeResult.Succeeded)
                    {
                        
                        _logger.LogError("Failed to remove existing permission claims for user {UserId}: {Errors}",
                            user.Id, string.Join(", ", removeResult.Errors.Select(e => e.Description)));
              
                        return ApplicationConstants.FailureMessage<List<string>>(null, "Failed to remove existing permissions before assigning new ones.");
                    }
                    _logger.LogInformation("Removed {Count} existing permission claims for user {UserId}.", claimsToRemove.Count, user.Id);
                }

                var claimsToAdd = request.Permissions
                                         .Distinct() 
                                         .Select(permissionName => new Claim(PermissionClaimType, permissionName))
                                         .ToList();

                if (claimsToAdd.Any())
                {

                    var addResult = await _userManager.AddClaimsAsync(user, claimsToAdd);
                    if (!addResult.Succeeded)
                    {
                        _logger.LogError("Failed to add new permission claims for user {UserId}: {Errors}",
                            user.Id, string.Join(", ", addResult.Errors.Select(e => e.Description)));
                        return ApplicationConstants.FailureMessage<List<string>>(null, "Failed to add one or more new permissions.");
                    }
                }


                _logger.LogInformation("Successfully updated permissions for user {UserId}. Assigned {Count} permissions.",
                    user.Id, claimsToAdd.Count);
                return ApplicationConstants.SuccessMessage(
                    claimsToAdd.Select(c => c.Value).ToList(), 
                    $"Permissions updated successfully for user '{user.UserName}'."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while assigning permissions to user {UserId}", request.UserId);
                return ApplicationConstants.FailureMessage<List<string>>(null, "An internal server error occurred while assigning permissions.");
            }
        }


    }
}

