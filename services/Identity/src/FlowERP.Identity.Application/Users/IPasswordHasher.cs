namespace FlowERP.Identity.Application.Users;

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string passwordHash, string password);
}