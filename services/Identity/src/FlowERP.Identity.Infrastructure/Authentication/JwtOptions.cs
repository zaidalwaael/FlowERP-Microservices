namespace FlowERP.Identity.Infrastructure.Authentication;

// يجمع إعدادات إنشاء والتحقق من رموز JWT.
public sealed class JwtOptions
{
    // اسم قسم الإعدادات في ملف appsettings.json.
    public const string SectionName = "Jwt";

    // اسم الجهة التي أصدرت رمز JWT.
    public string Issuer { get; init; } = string.Empty;

    // اسم الجهة أو التطبيق المسموح له باستخدام الرمز.
    public string Audience { get; init; } = string.Empty;

    // المفتاح السري المستخدم لتوقيع رمز JWT والتحقق منه.
    public string SecretKey { get; init; } = string.Empty;

    // مدة صلاحية Access Token بالدقائق.
    public int AccessTokenExpirationMinutes { get; init; }

    // مدة صلاحية Refresh Token بالأيام.
    public int RefreshTokenExpirationDays { get; init; }
}