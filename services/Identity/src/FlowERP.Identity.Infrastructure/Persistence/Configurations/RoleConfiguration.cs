using FlowERP.Identity.Domain.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowERP.Identity.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration
    : IEntityTypeConfiguration<Role>
{
    public void Configure(
        EntityTypeBuilder<Role> builder)
    {
        // Map the Role entity to the Roles table.
        builder.ToTable("Roles");

        // Configure the primary key.
        builder.HasKey(role => role.Id);

        // Configure the role name.
        builder.Property(role => role.Name)
            .HasMaxLength(100)
            .IsRequired();

        // Prevent duplicate role names.
        builder.HasIndex(role => role.Name)
            .IsUnique();
    }
}