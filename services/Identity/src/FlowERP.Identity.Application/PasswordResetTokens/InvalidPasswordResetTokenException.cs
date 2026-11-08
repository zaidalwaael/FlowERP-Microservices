namespace FlowERP.Identity.Application.PasswordResetTokens;

public sealed class InvalidPasswordResetTokenException
    : Exception
{
    public InvalidPasswordResetTokenException()
        : base("The password reset token is invalid or expired.")
    {
    }
}
