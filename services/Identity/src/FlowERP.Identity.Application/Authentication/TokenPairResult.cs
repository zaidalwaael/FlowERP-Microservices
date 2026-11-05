namespace FlowERP.Identity.Application.Authentication;

public sealed record TokenPairResult(
    string AccessToken,
    string RefreshToken);