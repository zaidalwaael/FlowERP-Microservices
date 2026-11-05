
using FlowERP.Identity.Application.Authorization;
using Microsoft.EntityFrameworkCore;

namespace FlowERP.Identity.Infrastructure.Persistence.Repositories;

public sealed class UserAuthorizationRepository(
    IdentityDbContext dbContext)
    : IUserAuthorizationRepository
{
    public async Task<IReadOnlyCollection<string>> GetRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Get the names of roles assigned to the user.
        return await (
            from userRole in dbContext.UserRoles
            join role in dbContext.Roles
                on userRole.RoleId equals role.Id
            where userRole.UserId == userId
            select role.Name
        )
        .Distinct()
        .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Get permissions granted through the user's roles.
        return await (
            from userRole in dbContext.UserRoles
            join rolePermission in dbContext.RolePermissions
                on userRole.RoleId equals rolePermission.RoleId
            join permission in dbContext.Permissions
                on rolePermission.PermissionId equals permission.Id
            where userRole.UserId == userId
            select permission.Name
        )
        .Distinct()
        .ToListAsync(cancellationToken);
    }
}
