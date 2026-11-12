
using FlowERP.Identity.Api.Contracts.Auth;
using FlowERP.Identity.Application.Authentication;
using FlowERP.Identity.Application.PasswordResetTokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
// Import endpoint rate limiting attributes.
using Microsoft.AspNetCore.RateLimiting;

namespace FlowERP.Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    LoginService loginService,
    RefreshTokenService refreshTokenService,
    LogoutService logoutService,
    ForgotPasswordService forgotPasswordService,
    ResetPasswordService resetPasswordService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-login")]
    [ProducesResponseType(
        typeof(TokenResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TokenResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await loginService.LoginAsync(
            request.Email,
            request.Password,
            cancellationToken);

        var response = new TokenResponse(
            result.AccessToken,
            result.RefreshToken);

        return Ok(response);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-refresh")]
    [ProducesResponseType(
        typeof(TokenResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TokenResponse>> Refresh(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await refreshTokenService.RefreshAsync(
            request.RefreshToken,
            cancellationToken);

        var response = new TokenResponse(
            result.AccessToken,
            result.RefreshToken);

        return Ok(response);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        await logoutService.LogoutAsync(
            request.RefreshToken,
            cancellationToken);

        return NoContent();
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await resetPasswordService.ResetAsync(
            request.Token,
            request.NewPassword,
            cancellationToken);

        return NoContent();
    }

    // Request a password reset link.
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await forgotPasswordService.RequestResetAsync(
            request.Email,
            cancellationToken);

        return Ok(new
        {
            Message =
                "If the account exists, a password reset link will be sent."
        });
    }
}
