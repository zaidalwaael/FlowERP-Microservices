using FlowERP.Identity.Application.Roles;
using FlowERP.Identity.Application.UserRoles;
using FlowERP.Identity.Domain.RolePermissions;
namespace FlowERP.Identity.Application.RolePermissions;
public sealed record PermissionResponseDto(Guid Id,string Name);
public sealed class ManageRolePermissionsService(IRoleRepository roles,IRolePermissionRepository assignments,IUnitOfWork uow) {
 private async Task EnsureRoleAsync(Guid id,CancellationToken ct) {
  if(id==Guid.Empty) throw new ArgumentException("Role ID is required.");
  if(await roles.GetByIdAsync(id,ct) is null) throw new RoleNotFoundException();
 }
 public async Task<IReadOnlyList<PermissionResponseDto>> GetAsync(Guid roleId,CancellationToken ct=default) {
  await EnsureRoleAsync(roleId,ct);
  var items=await assignments.GetByRoleIdAsync(roleId,ct);
  return items.Select(p=>new PermissionResponseDto(p.Id,p.Name)).ToList();
 }
 public async Task AssignAsync(Guid roleId,Guid permissionId,CancellationToken ct=default) {
  await EnsureRoleAsync(roleId,ct);
  if(permissionId==Guid.Empty) throw new ArgumentException("Permission ID is required.");
  if(await assignments.GetPermissionAsync(permissionId,ct) is null) throw new PermissionNotFoundException();
  if(await assignments.ExistsAsync(roleId,permissionId,ct)) throw new RolePermissionAlreadyAssignedException();
  await assignments.AddAsync(new RolePermission(roleId,permissionId),ct);
  await uow.SaveChangesAsync(ct);
 }
 public async Task RemoveAsync(Guid roleId,Guid permissionId,CancellationToken ct=default) {
  await EnsureRoleAsync(roleId,ct);
  if(permissionId==Guid.Empty) throw new ArgumentException("Permission ID is required.");
  if(await assignments.GetPermissionAsync(permissionId,ct) is null) throw new PermissionNotFoundException();
  if(!await assignments.RemoveAsync(roleId,permissionId,ct)) throw new RolePermissionNotAssignedException();
  await uow.SaveChangesAsync(ct);
 }
}