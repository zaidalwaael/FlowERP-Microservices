namespace FlowERP.Identity.Domain.UserRoles;

public sealed class UserRole
{
    private UserRole()
    {
        // Required by EF Core when loading a user-role relationship.
    }

    public UserRole(
        Guid userId,
        Guid roleId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException(
                "User ID is required.",
                nameof(userId));

        if (roleId == Guid.Empty)
            throw new ArgumentException(
                "Role ID is required.",
                nameof(roleId));

        UserId = userId;
        RoleId = roleId;
    }

    public Guid UserId { get; private set; }

    public Guid RoleId { get; private set; }
}