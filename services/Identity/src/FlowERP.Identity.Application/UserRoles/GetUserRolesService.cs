
using FlowERP.Identity.Application.Roles.DTOs;
using FlowERP.Identity.Application.Users;

namespace FlowERP.Identity.Application.UserRoles;

public sealed class GetUserRolesService(
    IUserRoleRepository userRoleRepository,
    IUserRepository userRepository)
{
    public async Task<IReadOnlyList<RoleResponseDto>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Validate the user ID.
        if (userId == Guid.Empty)
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));

        // Check whether the user exists.
        var user = await userRepository.GetByIdAsync(
            userId,
            cancellationToken);

        if (user is null)
            throw new UserNotFoundException();

        // Retrieve all roles assigned to the user.
        var roles = await userRoleRepository.GetRolesByUserIdAsync(
            userId,
            cancellationToken);

        // Map domain entities to response DTOs.
        return roles
            .Select(role => new RoleResponseDto(
                role.Id,
                role.Name))
            .ToList();
    }
}
