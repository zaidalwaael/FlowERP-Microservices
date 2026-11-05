using FlowERP.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowERP.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.Property(user => user.PasswordHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(user => user.IsActive)
            .IsRequired();

        builder.Property(user => user.CreatedAtUtc)
            .IsRequired();
    }
}