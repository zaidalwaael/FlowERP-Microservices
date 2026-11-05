using FlowERP.Identity.Domain.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowERP.Identity.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration
    : IEntityTypeConfiguration<Permission>
{
    public void Configure(
        EntityTypeBuilder<Permission> builder)
    {
        // Map the Permission entity to the Permissions table.
        builder.ToTable("Permissions");

        // Configure the primary key.
        builder.HasKey(permission => permission.Id);

        // Configure the permission name.
        builder.Property(permission => permission.Name)
            .HasMaxLength(150)
            .IsRequired();

        // Prevent duplicate permission names.
        builder.HasIndex(permission => permission.Name)
            .IsUnique();
    }
}