namespace PropertyManagement.API.Auth;

public sealed class DevAuthOptions
{
    public const string SectionName = "DevAuth";

    public string Username { get; init; } = "admin@property.local";
    public string Password { get; init; } = "Passw0rd!";
    public string DisplayName { get; init; } = "Local Admin";
    public string Role { get; init; } = "Administrator";
}
