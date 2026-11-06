using FlowERP.Identity.Application.Users;
using FlowERP.Identity.Application.UserRoles;
namespace FlowERP.Identity.Application.Authorization;
public sealed class GetUserPermissionsService(IUserRepository users,IUserAuthorizationRepository authorization) {
 public async Task<IReadOnlyCollection<string>> GetAsync(Guid userId,CancellationToken ct=default) {
  if(userId==Guid.Empty) throw new ArgumentException("User ID is required.",nameof(userId));
  if(await users.GetByIdAsync(userId,ct) is null) throw new UserNotFoundException();
  return await authorization.GetPermissionsAsync(userId,ct);
 }
}