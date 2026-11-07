namespace FlowERP.Identity.Application.Users;
public sealed record UserResponseDto(Guid Id,string Email,bool IsActive,DateTime CreatedAtUtc);