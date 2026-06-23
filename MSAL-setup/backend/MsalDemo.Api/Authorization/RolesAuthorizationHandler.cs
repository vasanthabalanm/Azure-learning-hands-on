using Microsoft.AspNetCore.Authorization;

namespace MsalDemo.Api.Authorization;

/// <summary>
/// Custom authorization requirement for Azure AD roles.
/// </summary>
public class RolesRequirement : IAuthorizationRequirement
{
    public string[] AllowedRoles { get; }

    public RolesRequirement(params string[] allowedRoles)
    {
        AllowedRoles = allowedRoles;
    }
}

/// <summary>
/// Authorization handler that explicitly checks the "roles" claim from Azure AD.
/// This bypasses issues with RoleClaimType configuration in Microsoft.Identity.Web.
/// </summary>
public class RolesAuthorizationHandler : AuthorizationHandler<RolesRequirement>
{
    private readonly ILogger<RolesAuthorizationHandler> _logger;

    public RolesAuthorizationHandler(ILogger<RolesAuthorizationHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        RolesRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            _logger.LogWarning("User is not authenticated");
            return Task.CompletedTask;
        }

        // Get roles from the "roles" claim (Azure AD format)
        var roleClaims = context.User.FindAll("roles").Select(c => c.Value).ToList();
        
        // Also check the standard role claim type as fallback
        if (!roleClaims.Any())
        {
            roleClaims = context.User.FindAll(System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value).ToList();
        }

        _logger.LogDebug("User roles from token: [{Roles}]", string.Join(", ", roleClaims));
        _logger.LogDebug("Required roles: [{RequiredRoles}]", string.Join(", ", requirement.AllowedRoles));

        // Check if user has any of the allowed roles
        var hasRole = requirement.AllowedRoles.Any(allowedRole => 
            roleClaims.Contains(allowedRole, StringComparer.OrdinalIgnoreCase));

        if (hasRole)
        {
            _logger.LogDebug("Authorization succeeded - user has required role");
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning(
                "Authorization failed - user roles [{UserRoles}] do not include any of [{RequiredRoles}]",
                string.Join(", ", roleClaims),
                string.Join(", ", requirement.AllowedRoles));
        }

        return Task.CompletedTask;
    }
}
