
using FlowERP.Identity.Application;
using FlowERP.Identity.Application.Authentication;
using FlowERP.Identity.Application.Authorization;
using FlowERP.Identity.Application.Roles;
using FlowERP.Identity.Application.RolePermissions;
using FlowERP.Identity.Application.UserRoles;
using FlowERP.Identity.Application.Users;
using FlowERP.Identity.Infrastructure.Authentication;
using FlowERP.Identity.Infrastructure.Persistence;
using FlowERP.Identity.Infrastructure.Persistence.Repositories;
using FlowERP.Identity.Infrastructure.Persistence.Seeding;
using FlowERP.Identity.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace FlowERP.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString)
    {
        // Register EF Core with the Identity service database.
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Register the database seeder.
        services.AddScoped<IdentityDataSeeder>();

        // Register persistence contracts.
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<
            IRefreshTokenRepository,
            RefreshTokenRepository>();

        // Register the repository for loading user roles and permissions.
        services.AddScoped<
            IUserAuthorizationRepository,
            UserAuthorizationRepository>();

        // Register role persistence.
        services.AddScoped<
            IRoleRepository,
            RoleRepository>();

        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();

        // Register user-role assignment persistence.
        services.AddScoped<
            IUserRoleRepository,
            UserRoleRepository>();

        // Use the same DbContext instance for repositories and Unit of Work.
        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<IdentityDbContext>());

        // Register the password hashing implementation and its options.
        services.AddOptions<PasswordHasherOptions>();

        services.AddScoped<
            IPasswordHasher,
            IdentityPasswordHasher>();

        // Bind the "Jwt" configuration section to JwtOptions.
        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));

        // Register the access token generator.
        services.AddScoped<
            ITokenGenerator,
            JwtTokenGenerator>();

        // Register the refresh token generator.
        services.AddScoped<
            IRefreshTokenGenerator,
            RefreshTokenGenerator>();

        // Read JWT settings used to validate incoming access tokens.
        var jwtOptions = configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>()
            ?? throw new InvalidOperationException(
                "JWT configuration is missing.");

        // Decode the Base64-encoded JWT signing key.
        var signingKeyBytes = Convert.FromBase64String(
            jwtOptions.SecretKey);

        // Register JWT Bearer authentication.
        services
            .AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Audience,

                        ValidateLifetime = true,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                signingKeyBytes)
                    };
            });

        // Register authorization without static permission policies.
        services.AddAuthorization();

        return services;
    }
}
