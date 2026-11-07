using System.ComponentModel.DataAnnotations;
namespace FlowERP.Identity.Api.Contracts.Users;
public sealed record UpdateUserEmailRequest([Required,EmailAddress,MaxLength(256)] string Email);
public sealed record SetUserActiveRequest(bool IsActive);