namespace FlowERP.Identity.Domain.RolePermissions;

public sealed class RolePermission
{
    private RolePermission()
    {
        // Required by EF Core when loading a role-permission relationship.
    }

    public RolePermission(
        Guid roleId,
        Guid permissionId)
    {
        if (roleId == Guid.Empty)
            throw new ArgumentException(
                "Role ID is required.",
                nameof(roleId));

        if (permissionId == Guid.Empty)
            throw new ArgumentException(
                "Permission ID is required.",
                nameof(permissionId));

        RoleId = roleId;
        PermissionId = permissionId;
    }

    public Guid RoleId { get; private set; }

    public Guid PermissionId { get; private set; }
}