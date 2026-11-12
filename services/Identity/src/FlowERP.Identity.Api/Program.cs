
using FlowERP.Identity.Api.Authorization;
using FlowERP.Identity.Api.ExceptionHandling;
using FlowERP.Identity.Application.Authentication;
using FlowERP.Identity.Application.Authorization;
using FlowERP.Identity.Application.PasswordResetTokens;
using FlowERP.Identity.Application.RolePermissions;
using FlowERP.Identity.Application.Roles;
using FlowERP.Identity.Application.UserRoles;
using FlowERP.Identity.Application.Users;
using FlowERP.Identity.Infrastructure;
using FlowERP.Identity.Infrastructure.Persistence.Seeding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;

// Import rate limiting middleware and algorithms.
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Register API controllers.
builder.Services.AddControllers();

// Register Swagger/OpenAPI services.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "bearer",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT Authorization header using the Bearer scheme."
        });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(
                "bearer",
                document)] = []
        });
});

// Register global exception handling.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();


// Configure rate limiting to protect authentication endpoints
// from excessive requests and brute-force attacks.
builder.Services.AddRateLimiter(options =>
{
    // Return HTTP 429 when the request limit is exceeded.
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;


    // Inform clients when they can retry after exceeding the rate limit.
    options.OnRejected = (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode =
            StatusCodes.Status429TooManyRequests;

        if (context.Lease.TryGetMetadata(
            MetadataName.RetryAfter,
            out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                Math.Ceiling(retryAfter.TotalSeconds).ToString(
                    System.Globalization.CultureInfo.InvariantCulture);
        }

        return ValueTask.CompletedTask;
    };



    // Limit login attempts independently for each client IP address.
    options.AddPolicy("auth-login", context =>
    {
        var ipAddress = context.Connection.RemoteIpAddress?
            .MapToIPv6()
            .ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ipAddress,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });


    // Limit password recovery requests independently for each client IP.
    options.AddPolicy("auth-forgot-password", context =>
    {
        var ipAddress = context.Connection.RemoteIpAddress?
            .MapToIPv6()
            .ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ipAddress,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(15),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });


    // Limit token refresh requests independently for each client IP.
    options.AddPolicy("auth-refresh", context =>
    {
        var ipAddress = context.Connection.RemoteIpAddress?
            .MapToIPv6()
            .ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ipAddress,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });



    // Limit password reset attempts independently for each client IP.
    options.AddPolicy("auth-reset-password", context =>
    {
        var ipAddress = context.Connection.RemoteIpAddress?
            .MapToIPv6()
            .ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ipAddress,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(15),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });





});


// Read the Identity service database connection string.
var connectionString = builder.Configuration
    .GetConnectionString("IdentityDb")
    ?? throw new InvalidOperationException(
        "The IdentityDb connection string is missing.");


// Register infrastructure services.
builder.Services.AddInfrastructure(
    builder.Configuration,
    connectionString);


// Register the dynamic permission policy provider.
builder.Services.AddSingleton<
    IAuthorizationPolicyProvider,
    PermissionAuthorizationPolicyProvider>();


// Register the handler responsible for validating permission claims.
builder.Services.AddScoped<
    IAuthorizationHandler,
    PermissionAuthorizationHandler>();

// Register application use cases.
builder.Services.AddScoped<CreateUserService>();
builder.Services.AddScoped<ManageUsersService>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<RefreshTokenService>();
builder.Services.AddScoped<LogoutService>();
builder.Services.AddScoped<ForgotPasswordService>();
builder.Services.AddScoped<ResetPasswordService>();

// Register role management use cases.
builder.Services.AddScoped<CreateRoleService>();
builder.Services.AddScoped<GetRoleByIdService>();
builder.Services.AddScoped<GetAllRolesService>();
builder.Services.AddScoped<ManageRolePermissionsService>();

// Register user-role management use cases.
builder.Services.AddScoped<AssignRoleService>();
builder.Services.AddScoped<GetUserRolesService>();
builder.Services.AddScoped<RemoveRoleService>();
builder.Services.AddScoped<GetUserPermissionsService>();

var app = builder.Build();

// Seed initial Identity data in Development only.
if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();

    var seeder = scope.ServiceProvider
        .GetRequiredService<IdentityDataSeeder>();

    await seeder.SeedAsync();
}

// Enable Swagger only in the Development environment.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Handle unhandled exceptions globally.
app.UseExceptionHandler();

// Redirect HTTP requests to HTTPS.
app.UseHttpsRedirection();


// Match incoming requests to API endpoints.
app.UseRouting();

// Authenticate the user from the access token.
app.UseAuthentication();

// Enforce per-endpoint rate limiting policies.
app.UseRateLimiter();

// Apply authorization rules.
app.UseAuthorization();

// Map HTTP requests to API controllers.
app.MapControllers();


app.Run();
