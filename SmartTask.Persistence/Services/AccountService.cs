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
    using SmartTask.Domain.Models;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using static SmartTask.Domain.Constants.Permissions;

    namespace SmartTasks.Infrastructure.Identity.Managers
    {
        public class AccountService : IAccountService
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly SignInManager<ApplicationUser> _signInManager;
            private readonly RoleManager<IdentityRole> _roleManager;
            private readonly JwtService _jwtService;
            private readonly ILogger<AccountService> _logger;
            private readonly IEntityManagerAsync _entityManager;

            public AccountService(
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager,
                RoleManager<IdentityRole> roleManager,
                JwtService jwtService,
                ILogger<AccountService> logger,IEntityManagerAsync entityManagerAsync)
            {
                _userManager = userManager;
                _signInManager = signInManager;
                _roleManager = roleManager;
                _jwtService = jwtService;
                _logger = logger;
                _entityManager = entityManagerAsync;
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

                    var result = await _signInManager.CheckPasswordSignInAsync(
                    user,
                    request.Password,
                    false
                );

                    if (!result.Succeeded)
                    {
                        if (result.IsLockedOut)
                        {
                            return ApplicationConstants.FailureMessage<LoginResponse>(
                                null,
                                "Account is locked."
                            );
                        }

                        if (result.IsNotAllowed)
                        {
                            return ApplicationConstants.FailureMessage<LoginResponse>(
                                null,
                                "Login not allowed. Confirm email or activate account."
                            );
                        }

                        return ApplicationConstants.FailureMessage<LoginResponse>(
                            null,
                            "Invalid Credentials"
                        );
                    }

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
            

        }
    }
}
