using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShiftEngine.Domain;
using ShiftEngine.Infrastructure.Auth;
using ShiftEngine.Infrastructure.Export;
using ShiftEngine.Infrastructure.Identity;
using ShiftEngine.Infrastructure.Persistence;
using ShiftEngine.Infrastructure.SickLeave;

namespace ShiftEngine.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddShiftInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddIdentity<AppUser, IdentityRole>(o =>
            {
                o.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        services.AddScoped<JwtTokenIssuer>();
        services.AddScoped<SickLeaveReplanService>();
        services.AddScoped<RosterExcelExportService>();
        return services;
    }
}
