namespace FlowERP.Identity.Api.Contracts.Auth;

public sealed record TokenResponse(
    string AccessToken,
    string RefreshToken);