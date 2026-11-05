namespace FlowERP.Identity.Application.Authentication;

using FlowERP.Identity.Application.Authentication;
public interface ITokenGenerator
{
    string GenerateAccessToken(
        Guid userId,
        string email,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions);
}
