namespace FlowERP.Identity.Application.Authentication;

public sealed class InvalidRefreshTokenException : Exception
{
    public InvalidRefreshTokenException()
        : base("The refresh token is invalid or expired.")
    {
    }
}