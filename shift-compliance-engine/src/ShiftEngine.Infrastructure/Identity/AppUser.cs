using Microsoft.AspNetCore.Identity;

namespace ShiftEngine.Infrastructure.Identity;

public class AppUser : IdentityUser
{
    public Guid TenantId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}
