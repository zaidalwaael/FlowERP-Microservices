using FlowERP.Identity.Domain.RefreshTokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowERP.Identity.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration
    : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(
        EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(refreshToken => refreshToken.Id);

        builder.Property(refreshToken => refreshToken.TokenHash)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(refreshToken => refreshToken.UserId)
            .IsRequired();

        builder.Property(refreshToken => refreshToken.ExpiresAtUtc)
            .IsRequired();

        builder.Property(refreshToken => refreshToken.CreatedAtUtc)
            .IsRequired();

        builder.Property(refreshToken => refreshToken.RevokedAtUtc)
            .IsRequired(false);

        builder.HasIndex(refreshToken => refreshToken.TokenHash)
            .IsUnique();

        builder.HasIndex(refreshToken => refreshToken.UserId);
    }
}