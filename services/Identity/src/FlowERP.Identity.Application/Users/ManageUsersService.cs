using FlowERP.Identity.Application.UserRoles;
namespace FlowERP.Identity.Application.Users;
public sealed class ManageUsersService(IUserRepository users,IUnitOfWork uow) {
 public async Task<IReadOnlyList<UserResponseDto>> GetAllAsync(CancellationToken ct=default) {
  var result=await users.GetAllAsync(ct);
  return result.Select(Map).ToList();
 }
 public async Task<UserResponseDto> GetAsync(Guid id,CancellationToken ct=default) {
  var user=await FindAsync(id,ct);return Map(user);
 }
 public async Task<UserResponseDto> UpdateEmailAsync(Guid id,string email,CancellationToken ct=default) {
  var user=await FindAsync(id,ct);
  if(string.IsNullOrWhiteSpace(email)||email.Trim().Length>256) throw new ArgumentException("Valid email is required.");
  var existing=await users.GetByEmailAsync(email,ct);
  if(existing is not null&&existing.Id!=id) throw new UserAlreadyExistsException(email);
  user.ChangeEmail(email);
  await uow.SaveChangesAsync(ct);return Map(user);
 }
 public async Task<UserResponseDto> SetActiveAsync(Guid id,bool active,CancellationToken ct=default) {
  var user=await FindAsync(id,ct);
  if(active) user.Activate();else user.Deactivate();
  await uow.SaveChangesAsync(ct);return Map(user);
 }
 private async Task<FlowERP.Identity.Domain.Users.User> FindAsync(Guid id,CancellationToken ct) {
  if(id==Guid.Empty) throw new ArgumentException("User ID is required.");
  return await users.GetByIdAsync(id,ct)??throw new UserNotFoundException();
 }
 private static UserResponseDto Map(FlowERP.Identity.Domain.Users.User u)=>new(u.Id,u.Email,u.IsActive,u.CreatedAtUtc);
}