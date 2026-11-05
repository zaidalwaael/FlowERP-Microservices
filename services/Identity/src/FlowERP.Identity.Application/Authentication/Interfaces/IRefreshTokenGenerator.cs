
namespace FlowERP.Identity.Application.Authentication;

public interface IRefreshTokenGenerator
{
    string Generate();

    string Hash(string refreshToken);

    DateTime GetExpirationDate();
}