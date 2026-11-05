
using FlowERP.Identity.Domain.UserRoles;
using FlowERP.Identity.Domain.Roles;

namespace FlowERP.Identity.Application.UserRoles;

public interface IUserRoleRepository
{
    // Check whether a role is already assigned to a user.
    Task<bool> ExistsAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default);

    // Track a new user-role assignment for insertion.
    Task AddAsync(
        UserRole userRole,
        CancellationToken cancellationToken = default);

    // Get all roles assigned to a specific user.
    Task<IReadOnlyList<Role>> GetRolesByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
