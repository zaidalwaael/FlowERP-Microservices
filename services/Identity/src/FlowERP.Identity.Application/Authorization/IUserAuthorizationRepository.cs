
namespace FlowERP.Identity.Application.Authorization;

public interface IUserAuthorizationRepository
{
    Task<IReadOnlyCollection<string>> GetRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<string>> GetPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
