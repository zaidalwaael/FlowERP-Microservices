using FlowERP.Identity.Domain.Roles;
using FlowERP.Identity.Domain.UserRoles;
using FlowERP.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowERP.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserRoleConfiguration
    : IEntityTypeConfiguration<UserRole>
{
    public void Configure(
        EntityTypeBuilder<UserRole> builder)
    {
        // Map the UserRole entity to the UserRoles table.
        builder.ToTable("UserRoles");

        // Use UserId and RoleId together as the composite primary key.
        builder.HasKey(userRole => new
        {
            userRole.UserId,
            userRole.RoleId
        });

        // Configure UserId as a foreign key to Users.Id.
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(userRole => userRole.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Link the UserRole entity to the User entity.
        // Automatically delete the user's role assignments when the user is deleted.
        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(userRole => userRole.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}