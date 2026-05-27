using System.Security.Claims;

namespace ShiftEngine.Api;

public static class HttpUserExtensions
{
    public static Guid GetTenantId(this ClaimsPrincipal user)
    {
        var c = user.FindFirst("tenant_id")?.Value;
        return Guid.TryParse(c, out var g) ? g : Guid.Empty;
    }

    public static string GetUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
}
