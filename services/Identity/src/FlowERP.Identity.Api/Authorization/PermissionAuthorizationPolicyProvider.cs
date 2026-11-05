
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace FlowERP.Identity.Api.Authorization;

public sealed class PermissionAuthorizationPolicyProvider(
    IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    private const string PolicyPrefix = "Permission:";

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(
        string policyName)
    {
        // Handle permission policies dynamically.
        if (policyName.StartsWith(
            PolicyPrefix,
            StringComparison.Ordinal))
        {
            var permission = policyName[PolicyPrefix.Length..];

            if (string.IsNullOrWhiteSpace(permission))
                return null;

            // Create a policy requiring authentication and the permission.
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(
                    new PermissionRequirement(permission))
                .Build();
        }

        // Preserve support for regular authorization policies.
        return await base.GetPolicyAsync(policyName);
    }

    public static string GetPolicyName(string permission)
    {
        return $"{PolicyPrefix}{permission}";
    }
}
