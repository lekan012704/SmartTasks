using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Identity.Services
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;
    using SmartTask.Application.Dto.Account;
    using SmartTask.Application.Interfaces;
    using SmartTask.Application.Wrappers;
    using SmartTask.Domain.Entities;
    using SmartTask.Identity.Models;
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
            private readonly JwtSettings _jwtSettings;

            public AccountService(
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager,
                IOptions<JwtSettings> jwtSettings)
            {
                _userManager = userManager;
                _signInManager = signInManager;
                _jwtSettings = jwtSettings.Value;
            }

            public async Task<Response<LoginResponse>> LoginAsync(LoginRequest request,CancellationToken cancellationToken)
            { 
                var response = new Response<LoginResponse>();
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                response.Message = "Invalid credentials";
                response.Succeeded = false;
                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
                if (!result.Succeeded)
                response.Message = "Invalid Password";
                response.Succeeded = false;

                var token = await GenerateJwtToken(user);

                var responses = new LoginResponse
                {
                    Token = token   
                };
                response.Message = "Login Sucessful";
                response.Data = responses;
                response.Succeeded = true;
                return response;
            }

            private async Task<string> GenerateJwtToken(ApplicationUser user)
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var claims = new[]
                {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email)
            };

                var roleClaims = new List<Claim>();
                foreach (var role in userRoles)
                    roleClaims.Add(new Claim(ClaimTypes.Role, role));

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims.Concat(roleClaims),
                    expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                    signingCredentials: creds
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
        }
    }

}
