using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartTask.Application.Interfaces;
using SmartTask.Domain.Constants;
using SmartTask.Domain.Models;
using SmartTask.Persistence.Contexts;
using System.Security.Claims;

namespace SmartTask.Identity.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<PermissionService> logger)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permission)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirst("id")?.Value;
                if (userId == null) return false;

                var identityUser = await _userManager.FindByIdAsync(userId);

                if (identityUser == null)
                {
                    _logger.LogWarning("No user associated with current principal.");
                    return false;
                }

                var userRoles = await _userManager.GetRolesAsync(identityUser);
                if (!userRoles.Any())
                {
                    _logger.LogWarning("User '{Email}' has no roles assigned.", identityUser.Email);
                    return false;
                }

                // Join RolePermissions with Permissions to check if any of user's roles have the target permission
                var hasPermission = await _db.RolePermission
                    .Include(rp => rp.Permission)
                    .Include(rp => rp.Role)
                    .AnyAsync(rp =>
                        userRoles.Contains(rp.Role.Name) &&
                        rp.Permission.Name == permission
                    );

                if (!hasPermission)
                {
                    _logger.LogInformation("Permission '{Permission}' not found for user '{Email}'.", permission, identityUser.Email);
                }

                return hasPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check permission '{Permission}' for user.", permission);
                return false;
            }
        }
        public Task<List<string>> GetPermissionsAsync(string searchTerm)
        {
            // Use the dynamically generated list from the Permissions class
            IEnumerable<string> permissionsQuery = Permissions.All;

            // Apply Filtering
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                permissionsQuery = permissionsQuery.Where(p =>
                    p.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // Apply Sorting and convert to List
            var sortedPermissions = permissionsQuery.OrderBy(p => p).ToList();

            // Return as a completed Task (since reflection is synchronous here)
            return Task.FromResult(sortedPermissions);
        }
    }
    }
