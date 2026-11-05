namespace FlowERP.Identity.Application.Authentication;

public sealed class InvalidCredentialsException()
    : Exception("Invalid email or password.");