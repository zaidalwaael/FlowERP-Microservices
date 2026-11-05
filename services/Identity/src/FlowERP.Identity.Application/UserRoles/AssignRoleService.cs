
using FlowERP.Identity.Application.Roles;
using FlowERP.Identity.Application.Users;
using FlowERP.Identity.Domain.UserRoles;

namespace FlowERP.Identity.Application.UserRoles;

public sealed class AssignRoleService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUserRoleRepository userRoleRepository,
    IUnitOfWork unitOfWork)
{
    public async Task AssignAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        // Validate the provided identifiers.
        if (userId == Guid.Empty)
            throw new ArgumentException(
                "User ID is required.",
                nameof(userId));

        if (roleId == Guid.Empty)
            throw new ArgumentException(
                "Role ID is required.",
                nameof(roleId));

        // Verify that the user exists.
        var user = await userRepository.GetByIdAsync(
            userId,
            cancellationToken);

        if (user is null)
            throw new UserNotFoundException();

        // Verify that the role exists.
        var role = await roleRepository.GetByIdAsync(
            roleId,
            cancellationToken);

        if (role is null)
            throw new RoleNotFoundException();

        // Prevent assigning the same role more than once.
        var alreadyAssigned = await userRoleRepository.ExistsAsync(
            userId,
            roleId,
            cancellationToken);

        if (alreadyAssigned)
            throw new UserRoleAlreadyAssignedException();

        // Create a new user-role relationship.
        var userRole = new UserRole(userId, roleId);

        // Track the new assignment for insertion.
        await userRoleRepository.AddAsync(
            userRole,
            cancellationToken);

        // Persist the assignment using Unit of Work.
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
