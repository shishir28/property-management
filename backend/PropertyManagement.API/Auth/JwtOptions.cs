namespace PropertyManagement.API.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "PropertyManagement";
    public string Audience { get; init; } = "PropertyManagement.Frontend";
    public string SigningKey { get; init; } = "dev-signing-key-change-me-please-1234567890";
    public int ExpirationMinutes { get; init; } = 120;
}
