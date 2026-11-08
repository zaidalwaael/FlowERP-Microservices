using System.ComponentModel.DataAnnotations;

namespace FlowERP.Identity.Api.Contracts.Auth;

public sealed class ResetPasswordRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [MinLength(12)]
    public string NewPassword { get; set; } = string.Empty;
}
