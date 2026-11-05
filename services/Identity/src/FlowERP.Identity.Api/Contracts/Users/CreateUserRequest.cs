namespace FlowERP.Identity.Api.Contracts.Users;

public sealed record CreateUserRequest(
    string Email,
    string Password);