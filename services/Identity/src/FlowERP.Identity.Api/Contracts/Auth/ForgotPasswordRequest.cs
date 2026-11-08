
using System.ComponentModel.DataAnnotations;

namespace FlowERP.Identity.Api.Contracts.Auth;

public sealed class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;
}
