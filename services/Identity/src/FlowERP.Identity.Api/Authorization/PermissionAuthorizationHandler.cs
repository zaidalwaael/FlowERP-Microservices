
using Microsoft.AspNetCore.Authorization;

namespace FlowERP.Identity.Api.Authorization;

public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Check whether the user is authenticated.
        if (context.User.Identity?.IsAuthenticated != true)
            return Task.CompletedTask;

        // Check whether the JWT contains the required permission.
        var hasPermission = context.User.HasClaim(
            claim =>
                claim.Type == "permission" &&
                claim.Value == requirement.Permission);

        if (hasPermission)
        {
            // Mark the authorization requirement as satisfied.
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
