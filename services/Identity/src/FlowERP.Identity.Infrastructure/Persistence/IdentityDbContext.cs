
using FlowERP.Identity.Application;
using FlowERP.Identity.Application.Roles;
using FlowERP.Identity.Domain.Permissions;
using FlowERP.Identity.Domain.RefreshTokens;
using FlowERP.Identity.Domain.RolePermissions;
using FlowERP.Identity.Domain.Roles;
using FlowERP.Identity.Domain.UserRoles;
using FlowERP.Identity.Domain.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FlowERP.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext(
    DbContextOptions<IdentityDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users =>
        Set<User>();

    public DbSet<RefreshToken> RefreshTokens =>
        Set<RefreshToken>();

    public DbSet<Role> Roles =>
        Set<Role>();

    public DbSet<Permission> Permissions =>
        Set<Permission>();

    public DbSet<UserRole> UserRoles =>
        Set<UserRole>();

    public DbSet<RolePermission> RolePermissions =>
        Set<RolePermission>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(IdentityDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (IsDuplicateRoleName(exception))
        {
            var roleEntry = exception.Entries
                .First(entry =>
                    entry.Entity is Role &&
                    entry.State == EntityState.Added);

            var role = (Role)roleEntry.Entity;

            throw new RoleAlreadyExistsException(role.Name);
        }
    }

    private static bool IsDuplicateRoleName(
        DbUpdateException exception)
    {
        // SQL Server duplicate key errors.
        if (exception.InnerException is not SqlException sqlException ||
            sqlException.Number is not (2601 or 2627))
        {
            return false;
        }

        // Ensure the failed operation includes a new Role.
        var hasAddedRole = exception.Entries.Any(entry =>
            entry.Entity is Role &&
            entry.State == EntityState.Added);

        if (!hasAddedRole)
            return false;

        // Match the unique index for Roles.Name.
        return sqlException.Message.Contains(
            "IX_Roles_Name",
            StringComparison.OrdinalIgnoreCase);
    }
}
