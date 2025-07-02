using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartTask.Domain.Constants;
using SmartTask.Persistence.Contexts;

public static class RolePermissionSeeder
{
    private static readonly Dictionary<string, List<string>> RolePermissions = new()
    {
        { "SuperAdmin", new()
            {
                Permissions.Task.Create,
                Permissions.Task.Edit,
                Permissions.Task.Delete,
                Permissions.Task.Comment,
                Permissions.Task.View,
                Permissions.Task.Archive,
                Permissions.Task.Assign,
                Permissions.Task.Upload,
                Permissions.User.Create,
                Permissions.User.AssignRole,
                Permissions.User.Delete,
                Permissions.User.View,
                Permissions.User.Activate,
                Permissions.User.Deactivate,
                Permissions.User.Edit,
                Permissions.Role.AssignPermissions,
                Permissions.Role.Delete,
                Permissions.Role.View,
                Permissions.Role.Create,
                Permissions.Role.Edit,
                Permissions.Report.View,
                Permissions.Report.Generate,
                Permissions.Report.Export,
                Permissions.Audit.View,
                Permissions.Audit.Download,
                Permissions.Settings.Update,
                Permissions.Settings.View
            }
        },
        { "CompanyAdmin", new()
            {
                Permissions.Task.Create,
                Permissions.Task.Assign,
                Permissions.Task.Edit,
                Permissions.Task.Delete,
                Permissions.Task.View,
                Permissions.Task.Archive,
                Permissions.User.Create,
                Permissions.User.AssignRole,
                Permissions.User.Delete,
                Permissions.User.View,
                Permissions.User.Activate,
                Permissions.User.Deactivate,
                Permissions.User.Edit,
                Permissions.Report.View
            }
        },
        { "Developer", new()
            {
                Permissions.Task.View,
                Permissions.Task.Edit,
                Permissions.Task.Comment
            }
        },
        { "User", new()
            {
                 Permissions.Task.View
            }
        }
    };

    public static async Task SeedAsync(
        ApplicationDbContext identityContext,
        RoleManager<IdentityRole> roleManager,
        ILogger logger)
    {
        var allPermissions = Permissions.All;

        var existingPermissions = await identityContext.Permission
            .Select(p => p.Name)
            .ToListAsync();

        var newPermissions = allPermissions
            .Except(existingPermissions)
            .Select(p => new SmartTask.Domain.Entities.Permission
            {
                Id = Guid.NewGuid(),
                Name = p
            })
            .ToList();

        if (newPermissions.Any())
        {
            await identityContext.Permission.AddRangeAsync(newPermissions);
            await identityContext.SaveChangesAsync();
            logger.LogInformation("Added {Count} new permissions.", newPermissions.Count);
        }

        var permissionMap = await identityContext.Permission
            .ToDictionaryAsync(p => p.Name, p => p.Id);

        foreach (var (roleName, assignedPermissions) in RolePermissions)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                logger.LogWarning("Role '{RoleName}' not found. Skipping.", roleName);
                continue;
            }

            foreach (var permName in assignedPermissions)
            {
                if (!permissionMap.TryGetValue(permName, out var permissionId))
                {
                    logger.LogWarning("Permission '{Permission}' not found. Skipping.", permName);
                    continue;
                }

                var alreadyAssigned = await identityContext.RolePermission
                    .AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permissionId);

                if (!alreadyAssigned)
                {
                    await identityContext.RolePermission.AddAsync(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permissionId
                    });

                    logger.LogInformation("Assigned permission '{Permission}' to role '{Role}'", permName, roleName);
                }
            }
        }

        await identityContext.SaveChangesAsync();
        logger.LogInformation("Role-permission assignments completed.");
    }
}
    