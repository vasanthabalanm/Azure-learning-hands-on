namespace MsalDemo.Api.Authorization;

/// <summary>
/// Role constants matching Azure AD App Roles.
/// These must match the "value" field in your Azure AD app registration manifest.
/// </summary>
public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string User = "User";
}
