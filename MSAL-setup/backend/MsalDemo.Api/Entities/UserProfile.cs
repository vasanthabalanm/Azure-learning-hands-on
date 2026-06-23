namespace MsalDemo.Api.Entities;

/// <summary>
/// Stores additional user profile data synced from Azure AD.
/// The ObjectId links to the user's Azure AD object identifier.
/// </summary>
public class UserProfile
{
    public int Id { get; set; }

    /// <summary>
    /// Azure AD Object ID (oid claim from JWT).
    /// </summary>
    public required string ObjectId { get; set; }

    public string? Email { get; set; }

    public string? DisplayName { get; set; }

    /// <summary>
    /// Tenant ID the user belongs to.
    /// </summary>
    public string? TenantId { get; set; }

    public DateTime FirstLoginAt { get; set; }

    public DateTime LastLoginAt { get; set; }
}
