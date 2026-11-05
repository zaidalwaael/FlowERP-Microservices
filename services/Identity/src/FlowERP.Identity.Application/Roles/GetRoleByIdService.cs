
using FlowERP.Identity.Application.Roles.DTOs;

namespace FlowERP.Identity.Application.Roles;

public sealed class GetRoleByIdService(
    IRoleRepository roleRepository)
{
    public async Task<RoleResponseDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // Retrieve the role from the repository.
        var role = await roleRepository.GetByIdAsync(
            id,
            cancellationToken);

        // Return null if the role does not exist.
        if (role is null)
            return null;

        // Map the domain entity to a response DTO.
        return new RoleResponseDto(
            role.Id,
            role.Name);
    }
}
