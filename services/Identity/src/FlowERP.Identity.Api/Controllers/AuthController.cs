using FlowERP.Identity.Api.Contracts.Auth;
using FlowERP.Identity.Application.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace FlowERP.Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    LoginService loginService,
    RefreshTokenService refreshTokenService,
    LogoutService logoutService) : ControllerBase
{
    [HttpPost("login")]
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
}