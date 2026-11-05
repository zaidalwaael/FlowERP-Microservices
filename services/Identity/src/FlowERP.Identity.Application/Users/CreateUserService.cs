using FlowERP.Identity.Domain.Users;

namespace FlowERP.Identity.Application.Users;

public sealed class CreateUserService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
{
    public async Task<Guid> CreateAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(
                "Email is required.",
                nameof(email));

        if (string.IsNullOrWhiteSpace(password) || password.Length < 12)
            throw new ArgumentException(
                "Password must contain at least 12 characters.",
                nameof(password));

        var existingUser = await userRepository.GetByEmailAsync(
            email,
            cancellationToken);

        if (existingUser is not null)
            throw new UserAlreadyExistsException(email);

        var user = new User(
            Guid.NewGuid(),
            email,
            passwordHasher.Hash(password));

        await userRepository.AddAsync(
            user,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        return user.Id;
    }
}