
using FlowERP.Identity.Domain.Roles;

namespace FlowERP.Identity.Application.Roles;

public sealed class CreateRoleService(
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Guid> CreateAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        // Validate the role name.
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Role name is required.",
                nameof(name));

        var normalizedName = name.Trim();

        if (normalizedName.Length > 100)
            throw new ArgumentException(
                "Role name cannot exceed 100 characters.",
                nameof(name));

        // Check whether the role already exists.
        var existingRole = await roleRepository.GetByNameAsync(
            normalizedName,
            cancellationToken);

        if (existingRole is not null)
            throw new RoleAlreadyExistsException(normalizedName);

        // Create the new role.
        var role = new Role(
            Guid.NewGuid(),
            normalizedName);

        // Track the role for insertion.
        await roleRepository.AddAsync(
            role,
            cancellationToken);

        // Persist changes to the database.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return role.Id;
    }
}
