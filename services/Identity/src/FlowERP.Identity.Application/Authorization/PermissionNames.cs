
namespace FlowERP.Identity.Application.Authorization;

public static class PermissionNames
{
    public static class Users
    {
        public const string Create = "users.create";
        public const string Read = "users.read";
        public const string Update = "users.update";
        public const string Deactivate = "users.deactivate";
    }

    public static class Roles
    {
        public const string Create = "roles.create";
        public const string Read = "roles.read";
        public const string Assign = "roles.assign";
        public const string ManagePermissions = "roles.permissions.manage";
    }

    public static class Permissions
    {
        public const string Read = "permissions.read";
    }
}
