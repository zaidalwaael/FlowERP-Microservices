namespace FlowERP.Identity.Application.PasswordResetTokens;

public sealed class PasswordResetOptions
{
    public const string SectionName = "PasswordReset";

    public bool EmailEnabled { get; set; }

    public string ResetPasswordUrl { get; set; } = string.Empty;
}
