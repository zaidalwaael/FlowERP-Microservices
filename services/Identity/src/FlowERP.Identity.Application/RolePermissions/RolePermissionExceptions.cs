namespace FlowERP.Identity.Application.RolePermissions;
public sealed class PermissionNotFoundException():Exception("Permission not found.");
public sealed class RolePermissionAlreadyAssignedException():Exception("Permission already assigned to role.");
public sealed class RolePermissionNotAssignedException():Exception("Permission not assigned to role.");