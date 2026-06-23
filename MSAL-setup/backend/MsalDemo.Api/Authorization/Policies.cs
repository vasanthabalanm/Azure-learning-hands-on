namespace MsalDemo.Api.Authorization;

/// <summary>
/// Authorization policy names used throughout the API.
/// </summary>
public static class Policies
{
    public const string AdminOnly = nameof(AdminOnly);
    public const string ManagerOrAbove = nameof(ManagerOrAbove);
    public const string UserOrAbove = nameof(UserOrAbove);
}
