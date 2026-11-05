namespace FlowERP.Identity.Application.Users;

public sealed class UserAlreadyExistsException(string email)
    : Exception($"A user with email '{email}' already exists.");