using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Identity.Services
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;
    using SmartTask.Application.Command;
    using SmartTask.Application.Constants;
    using SmartTask.Application.Dto.Account;
    using SmartTask.Application.Dto.Role;
    using SmartTask.Application.Interfaces;
    using SmartTask.Application.Wrappers;
    using SmartTask.Domain.Constants;
    using SmartTask.Domain.Entities;
    using SmartTask.Identity.Contexts;
    using SmartTask.Identity.Models;
    using SmartTask.Shared.Helpers;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;

    namespace SmartTasks.Infrastructure.Identity.Managers
    {
        public class AccountService : IAccountService
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly SignInManager<ApplicationUser> _signInManager;
            private readonly RoleManager<IdentityRole> _roleManager;
            private readonly IdentityContext _identityContext;
            private readonly JwtService _jwtService;
            private readonly ILogger<AccountService> _logger;

            public AccountService(
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager,
                RoleManager<IdentityRole> roleManager,
                IdentityContext identityContext,
                JwtService jwtService,
                ILogger<AccountService> logger)
            {
                _userManager = userManager;
                _signInManager = signInManager;
                _roleManager = roleManager;
                _identityContext = identityContext;
                _jwtService = jwtService;
                _logger = logger;
            }

            public async Task<Response<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
            {
                try
                {
                    var response = new Response<LoginResponse>();
                    var user = await _userManager.FindByEmailAsync(request.Email);
                    if (user == null)
                    {
                        response.Message = "Invalid Credentials";
                        response.Succeeded = false;
                        return response;
                    }

                    var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
                    if (!result.Succeeded)
                        return ApplicationConstants.FailureMessage<LoginResponse>(null, "Unsuccessful");

                    var token = await _jwtService.GenerateToken(user);
                    var handler = new JwtSecurityTokenHandler();
                    var tokenString = handler.WriteToken(token);

                    // Extract permissions from the token claims
                    var permissionClaims = token.Claims
                        .Where(c => c.Type == "permission")
                        .Select(c => c.Value)
                        .Distinct()
                        .ToList();

                    var loginResponse = new LoginResponse
                    {
                        Token = tokenString,
                        Permissions = permissionClaims
                    };

                    return ApplicationConstants.SuccessMessage(loginResponse, "Successful");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during login");
                    return ApplicationConstants.FailureMessage<LoginResponse>(null, "An error occurred while processing your request.");
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
                    var permissions = await _identityContext.Permission
                        .Where(p => request.Permissions.Contains(p.Name))
                        .ToListAsync();

                    var addedPermissions = new List<string>();

                    foreach (var permission in permissions)
                    {
                        var exists = await _identityContext.RolePermission
                            .AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);

                        if (!exists)
                        {
                            _identityContext.RolePermission.Add(new RolePermission
                            {
                                RoleId = role.Id,
                                PermissionId = permission.Id
                            });

                            addedPermissions.Add(permission.Name);
                        }
                    }

                    if (addedPermissions.Any())
                    {
                        await _identityContext.SaveChangesAsync();
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
}
