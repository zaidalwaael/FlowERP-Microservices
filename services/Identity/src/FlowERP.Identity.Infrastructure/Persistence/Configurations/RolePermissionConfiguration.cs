using FlowERP.Identity.Domain.Permissions;
using FlowERP.Identity.Domain.RolePermissions;
using FlowERP.Identity.Domain.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowERP.Identity.Infrastructure.Persistence.Configurations;

public sealed class RolePermissionConfiguration
    : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(
        EntityTypeBuilder<RolePermission> builder)
    {
        // Map the RolePermission entity to the RolePermissions table.
        builder.ToTable("RolePermissions");

        // Use RoleId and PermissionId together as the composite primary key.
        builder.HasKey(rolePermission => new
        {
            rolePermission.RoleId,
            rolePermission.PermissionId
        });

        // Configure RoleId as a foreign key to Roles.Id.
        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(rolePermission => rolePermission.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure PermissionId as a foreign key to Permissions.Id.
        builder.HasOne<Permission>()
            .WithMany()
            .HasForeignKey(rolePermission => rolePermission.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}