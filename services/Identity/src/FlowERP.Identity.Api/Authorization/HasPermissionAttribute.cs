
using Microsoft.AspNetCore.Authorization;

namespace FlowERP.Identity.Api.Authorization;

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission)
    {
        // Generate the dynamic policy name for the required permission.
        Policy = PermissionAuthorizationPolicyProvider
            .GetPolicyName(permission);
    }
}
