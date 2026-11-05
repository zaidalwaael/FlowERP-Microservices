
using FlowERP.Identity.Application.Authorization;
using FlowERP.Identity.Domain.Permissions;
using FlowERP.Identity.Domain.RolePermissions;
using FlowERP.Identity.Domain.Roles;
using FlowERP.Identity.Domain.UserRoles;
using Microsoft.EntityFrameworkCore;

namespace FlowERP.Identity.Infrastructure.Persistence.Seeding;

public sealed class IdentityDataSeeder(
    IdentityDbContext dbContext)
{
    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        // Ensure the Admin role exists.
        await SeedAdminRoleAsync(cancellationToken);

        // Ensure the initial permissions exist.
        await SeedPermissionsAsync(cancellationToken);

        // Assign permissions to the Admin role.
        await SeedAdminRolePermissionsAsync(cancellationToken);

        // Assign the Admin role to the development admin user.
        await SeedAdminUserRoleAsync(cancellationToken);
    }

    private async Task SeedAdminRoleAsync(
        CancellationToken cancellationToken)
    {
        // Check whether the Admin role already exists.
        var adminRoleExists = await dbContext.Roles
            .AnyAsync(
                role => role.Name == "Admin",
                cancellationToken);

        if (adminRoleExists)
            return;

        // Create the initial Admin role.
        var adminRole = new Role(
            Guid.NewGuid(),
            "Admin");

        await dbContext.Roles.AddAsync(
            adminRole,
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedPermissionsAsync(
        CancellationToken cancellationToken)
    {
        // Define the initial Identity permissions.
        string[] permissionNames =
        [
            // User management permissions.
            PermissionNames.Users.Create,
            PermissionNames.Users.Read,
            PermissionNames.Users.Update,
            PermissionNames.Users.Deactivate,

            // Role management permissions.
            PermissionNames.Roles.Create,
            PermissionNames.Roles.Read,
            PermissionNames.Roles.Assign,
            PermissionNames.Roles.ManagePermissions,

            // Permission management permissions.
            PermissionNames.Permissions.Read
        ];

        // Load the names of permissions already stored.
        var existingPermissionNames = await dbContext.Permissions
            .AsNoTracking()
            .Where(permission =>
                permissionNames.Contains(permission.Name))
            .Select(permission => permission.Name)
            .ToListAsync(cancellationToken);

        // Convert existing names to a HashSet for fast lookups.
        var existingNames = existingPermissionNames.ToHashSet();

        // Add only permissions that do not already exist.
        foreach (var permissionName in permissionNames)
        {
            if (existingNames.Contains(permissionName))
                continue;

            var permission = new Permission(
                Guid.NewGuid(),
                permissionName);

            await dbContext.Permissions.AddAsync(
                permission,
                cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedAdminRolePermissionsAsync(
        CancellationToken cancellationToken)
    {
        // Find the Admin role.
        var adminRole = await dbContext.Roles
            .SingleAsync(
                role => role.Name == "Admin",
                cancellationToken);

        // Load all available permissions.
        var permissions = await dbContext.Permissions
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Load permissions already assigned to Admin.
        var assignedPermissionIds = await dbContext.RolePermissions
            .AsNoTracking()
            .Where(rolePermission =>
                rolePermission.RoleId == adminRole.Id)
            .Select(rolePermission =>
                rolePermission.PermissionId)
            .ToListAsync(cancellationToken);

        // Convert assigned IDs to a HashSet for fast lookups.
        var assignedIds = assignedPermissionIds.ToHashSet();

        // Assign only missing permissions to Admin.
        foreach (var permission in permissions)
        {
            if (assignedIds.Contains(permission.Id))
                continue;

            var rolePermission = new RolePermission(
                adminRole.Id,
                permission.Id);

            await dbContext.RolePermissions.AddAsync(
                rolePermission,
                cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedAdminUserRoleAsync(
        CancellationToken cancellationToken)
    {
        // Find the existing development admin user.
        var adminUser = await dbContext.Users
            .SingleOrDefaultAsync(
                user => user.Email == "admin@flowerp.com",
                cancellationToken);

        if (adminUser is null)
            return;

        // Find the Admin role.
        var adminRole = await dbContext.Roles
            .SingleAsync(
                role => role.Name == "Admin",
                cancellationToken);

        // Check whether the user already has the Admin role.
        var alreadyAssigned = await dbContext.UserRoles
            .AnyAsync(
                userRole =>
                    userRole.UserId == adminUser.Id &&
                    userRole.RoleId == adminRole.Id,
                cancellationToken);

        if (alreadyAssigned)
            return;

        // Assign the Admin role to the existing user.
        var userRole = new UserRole(
            adminUser.Id,
            adminRole.Id);

        await dbContext.UserRoles.AddAsync(
            userRole,
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
