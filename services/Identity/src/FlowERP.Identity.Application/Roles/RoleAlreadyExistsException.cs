
namespace FlowERP.Identity.Application.Roles;

public sealed class RoleAlreadyExistsException(string roleName)
    : Exception($"Role '{roleName}' already exists.");
