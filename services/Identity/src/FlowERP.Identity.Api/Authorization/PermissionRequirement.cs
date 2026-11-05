using Microsoft.AspNetCore.Authorization;

namespace FlowERP.Identity.Api.Authorization;

public sealed class PermissionRequirement(string permission)
    : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}