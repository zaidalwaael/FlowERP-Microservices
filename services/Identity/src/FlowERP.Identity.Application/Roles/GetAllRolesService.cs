using FlowERP.Identity.Application.Roles.DTOs;

namespace FlowERP.Identity.Application.Roles;

public sealed class GetAllRolesService(
    IRoleRepository roleRepository)
{
    public async Task<IReadOnlyList<RoleResponseDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        // Retrieve all roles from the repository.
        var roles = await roleRepository.GetAllAsync(
            cancellationToken);

        // Map domain entities to response DTOs.
        return roles
            .Select(role => new RoleResponseDto(
                role.Id,
                role.Name))
            .ToList();
    }
}