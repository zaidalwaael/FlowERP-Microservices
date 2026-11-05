using FlowERP.Identity.Application.Roles;
using FlowERP.Identity.Application.Users;

namespace FlowERP.Identity.Application.UserRoles;

public sealed class RemoveRoleService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUserRoleRepository userRoleRepository,
    IUnitOfWork unitOfWork)
{
    public async Task RemoveAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required.", nameof(userId));

        if (roleId == Guid.Empty)
            throw new ArgumentException("Role ID is required.", nameof(roleId));

        if (await userRepository.GetByIdAsync(userId, cancellationToken) is null)
            throw new UserNotFoundException();

        if (await roleRepository.GetByIdAsync(roleId, cancellationToken) is null)
            throw new RoleNotFoundException();

        var removed = await userRoleRepository.RemoveAsync(userId, roleId, cancellationToken);

        if (!removed)
            throw new UserRoleNotAssignedException();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
