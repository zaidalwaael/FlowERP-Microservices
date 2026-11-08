
namespace FlowERP.Identity.Application.PasswordResetTokens;

public interface IPasswordResetTokenGenerator
{
    string Generate();

    string Hash(string token);

    DateTime GetExpirationDate();
}
