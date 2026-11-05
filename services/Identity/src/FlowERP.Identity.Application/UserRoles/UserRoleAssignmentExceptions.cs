namespace FlowERP.Identity.Application.UserRoles;

// Thrown when the specified user does not exist.
public sealed class UserNotFoundException : Exception
{
    public UserNotFoundException()
        : base("The specified user was not found.")
    {
    }
}

// Thrown when the specified role does not exist.
public sealed class RoleNotFoundException : Exception
{
    public RoleNotFoundException()
        : base("The specified role was not found.")
    {
    }
}

// Thrown when the user already has the specified role.
public sealed class UserRoleAlreadyAssignedException : Exception
{
    public UserRoleAlreadyAssignedException()
        : base("The specified role is already assigned to this user.")
    {
    }
}

public sealed class UserRoleNotAssignedException : Exception
{
    public UserRoleNotAssignedException()
        : base("The specified role is not assigned to this user.") { }
}
