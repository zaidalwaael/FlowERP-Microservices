using FlowERP.Identity.Application.UserRoles;
using FlowERP.Identity.Domain.UserRoles;

using FlowERP.Identity.Application.UserRoles;
using FlowERP.Identity.Domain.Roles;
using FlowERP.Identity.Domain.UserRoles;
using Microsoft.EntityFrameworkCore;

namespace FlowERP.Identity.Infrastructure.Persistence.Repositories;

public sealed class UserRoleRepository(
    IdentityDbContext dbContext) : IUserRoleRepository
{
    public async Task<bool> ExistsAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        // Check whether the user already has the specified role.
        return await dbContext.UserRoles
            .AnyAsync(
                userRole =>
                    userRole.UserId == userId &&
                    userRole.RoleId == roleId,
                cancellationToken);
    }

    public async Task AddAsync(
        UserRole userRole,
        CancellationToken cancellationToken = default)
    {
        // Track the new user-role assignment for insertion.
        await dbContext.UserRoles.AddAsync(
            userRole,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Role>> GetRolesByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Get all roles assigned to the specified user.
        return await (
            from userRole in dbContext.UserRoles
            join role in dbContext.Roles
                on userRole.RoleId equals role.Id
            where userRole.UserId == userId
            orderby role.Name
            select role
        )
        .AsNoTracking()
        .ToListAsync(cancellationToken);
    }
}
