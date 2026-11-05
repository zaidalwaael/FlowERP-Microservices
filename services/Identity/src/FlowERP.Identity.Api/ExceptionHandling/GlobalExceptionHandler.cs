using FlowERP.Identity.Application.Authentication;
using FlowERP.Identity.Application.Roles;
using FlowERP.Identity.Application.UserRoles;
using FlowERP.Identity.Application.Users;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FlowERP.Identity.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            UserAlreadyExistsException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "User already exists",
                Detail = exception.Message
            },

            RoleAlreadyExistsException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Role already exists",
                Detail = exception.Message
            },

            UserNotFoundException => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "User not found",
                Detail = exception.Message
            },

            RoleNotFoundException => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Role not found",
                Detail = exception.Message
            },

            UserRoleAlreadyAssignedException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Role already assigned",
                Detail = exception.Message
            },

            InvalidCredentialsException => new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid credentials",
                Detail = exception.Message
            },

            InvalidRefreshTokenException => new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid refresh token",
                Detail = exception.Message
            },

            ArgumentException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid request",
                Detail = exception.Message
            },

            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Server error",
                Detail = "An unexpected error occurred."
            }
        };

        if (problemDetails.Status >= 500)
        {
            logger.LogError(
                exception,
                "An unexpected error occurred while processing the request.");
        }
        else
        {
            logger.LogWarning(
                exception,
                "Request failed with status code {StatusCode}.",
                problemDetails.Status);
        }

        httpContext.Response.StatusCode =
            problemDetails.Status!.Value;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }
}