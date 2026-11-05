using FlowERP.Identity.Domain.Roles;

namespace FlowERP.Identity.Application.Roles;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<Role?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    // Retrieves all roles from the database.
    Task<IReadOnlyList<Role>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Role role,
        CancellationToken cancellationToken = default);
}