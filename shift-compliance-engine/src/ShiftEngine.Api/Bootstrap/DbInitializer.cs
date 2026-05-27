using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShiftEngine.Domain;
using ShiftEngine.Domain.Entities;
using ShiftEngine.Infrastructure.Identity;
using ShiftEngine.Infrastructure.Persistence;

namespace ShiftEngine.Api.Bootstrap;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var r in new[] { SecurityRoles.Admin, SecurityRoles.Planner, SecurityRoles.Manager, SecurityRoles.Employee, SecurityRoles.WorksCouncilAuditor })
        {
            if (!await roleManager.RoleExistsAsync(r))
                await roleManager.CreateAsync(new IdentityRole(r));
        }
    }
}
