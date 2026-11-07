using FlowERP.Identity.Domain.Permissions;
using FlowERP.Identity.Domain.RolePermissions;
namespace FlowERP.Identity.Application.RolePermissions;
public interface IRolePermissionRepository {
 Task<Permission?> GetPermissionAsync(Guid id,CancellationToken ct=default);
 Task<IReadOnlyList<Permission>> GetByRoleIdAsync(Guid roleId,CancellationToken ct=default);
 Task<bool> ExistsAsync(Guid roleId,Guid permissionId,CancellationToken ct=default);
 Task AddAsync(RolePermission assignment,CancellationToken ct=default);
 Task<bool> RemoveAsync(Guid roleId,Guid permissionId,CancellationToken ct=default);
}