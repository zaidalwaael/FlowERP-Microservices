using FlowERP.Identity.Application.Roles;
using FlowERP.Identity.Domain.Roles;
using Microsoft.EntityFrameworkCore;

namespace FlowERP.Identity.Infrastructure.Persistence.Repositories;

public sealed class RoleRepository(
    IdentityDbContext dbContext) : IRoleRepository
{
    public async Task<Role?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        // Find a role by its name.
        return await dbContext.Roles
            .AsNoTracking()
            .SingleOrDefaultAsync(
                role => role.Name == name,
                cancellationToken);
    }

    public async Task<Role?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // Find a role by its unique identifier.
        return await dbContext.Roles
            .AsNoTracking()
            .SingleOrDefaultAsync(
                role => role.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Role>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        // Retrieve all roles ordered by name without tracking.
        return await dbContext.Roles
            .AsNoTracking()
            .OrderBy(role => role.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Role role,
        CancellationToken cancellationToken = default)
    {
        // Track the new role for insertion.
        await dbContext.Roles.AddAsync(
            role,
            cancellationToken);
    }
}