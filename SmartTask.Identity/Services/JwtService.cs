using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartTask.Domain.Constants;
using SmartTask.Domain.Entities;
using SmartTask.Identity.Contexts;
using SmartTask.Identity.Migrations;
using SmartTask.Identity.Models;
using SmartTask.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Identity.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IdentityContext _identitycontext;
        private readonly JwtSettings _jwtSettings;
        public JwtService(IConfiguration configuration, UserManager<ApplicationUser> userManager, IdentityContext identitycontext, IOptions<JwtSettings> jwtSettings)
        {
            _configuration = configuration;
            _userManager = userManager;
            _identitycontext = identitycontext;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<JwtSecurityToken> GenerateToken(ApplicationUser user)
        {
            try
            {
                var userClaims = await _userManager.GetClaimsAsync(user);
                var roles = await _userManager.GetRolesAsync(user);
                var primaryRole = roles.FirstOrDefault();
                var userRole = await (from u in _identitycontext.UserRoles
                                      join r in _identitycontext.Roles on u.RoleId equals r.Id
                                      where u.UserId == user.Id
                                      select new
                                      {
                                          u.UserId,
                                          u.RoleId,
                                          r.Name
                                      }).FirstOrDefaultAsync();

                var roleId = userRole?.RoleId;
                var roleName = userRole?.Name;
                var permissionClaims = await _identitycontext.RolePermission
                    .Where(rp => rp.RoleId == roleId)
                    .Include(rp => rp.Permission)
                    .Select(rp => new Claim("permission", rp.Permission.Name))
                    .ToListAsync();
                var roleClaims = roles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();
                string ipAddress = IpHelper.GetIpAddress();
                var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

            new Claim("UserId", user.Id),
            new Claim("Email", user.Email ?? ""),
            new Claim("UserName", user.UserName ?? ""),
            new Claim("CompanyId", user.CompanyId?.ToString() ?? ""),
            new Claim("Role", roleName),
            new Claim("RoleName", roleName),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("id", user.Id),
            new Claim("ip", ipAddress),
            new Claim("roleid", roleId ?? "")
        }
                .Union(userClaims)
                .Union(roleClaims)
                .Union(permissionClaims);

                // ✅ Signing
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                return new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                    signingCredentials: creds
                );
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error generating JWT token", ex);
            }
        }



    }
}
        