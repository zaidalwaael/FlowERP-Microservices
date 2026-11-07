using FlowERP.Identity.Application.RolePermissions;
using FlowERP.Identity.Domain.Permissions;
using FlowERP.Identity.Domain.RolePermissions;
using Microsoft.EntityFrameworkCore;
namespace FlowERP.Identity.Infrastructure.Persistence.Repositories;
public sealed class RolePermissionRepository(IdentityDbContext db):IRolePermissionRepository {
 public Task<Permission?> GetPermissionAsync(Guid id,CancellationToken ct=default)=>db.Permissions.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id,ct);
 public async Task<IReadOnlyList<Permission>> GetByRoleIdAsync(Guid roleId,CancellationToken ct=default)=>
  await (from rp in db.RolePermissions join p in db.Permissions on rp.PermissionId equals p.Id where rp.RoleId==roleId orderby p.Name select p).AsNoTracking().ToListAsync(ct);
 public Task<bool> ExistsAsync(Guid roleId,Guid permissionId,CancellationToken ct=default)=>db.RolePermissions.AnyAsync(x=>x.RoleId==roleId&&x.PermissionId==permissionId,ct);
 public async Task AddAsync(RolePermission assignment,CancellationToken ct=default)=>await db.RolePermissions.AddAsync(assignment,ct);
 public async Task<bool> RemoveAsync(Guid roleId,Guid permissionId,CancellationToken ct=default) {
  var item=await db.RolePermissions.FirstOrDefaultAsync(x=>x.RoleId==roleId&&x.PermissionId==permissionId,ct);
  if(item is null) return false;
  db.RolePermissions.Remove(item);return true;
 }
}