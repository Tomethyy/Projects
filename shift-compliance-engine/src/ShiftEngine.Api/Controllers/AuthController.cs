using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShiftEngine.Infrastructure.Auth;
using ShiftEngine.Infrastructure.Identity;

namespace ShiftEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(SignInManager<AppUser> signIn, UserManager<AppUser> users, JwtTokenIssuer jwt) : ControllerBase
{
    public record LoginRequest(string Email, string Password);
    public record LoginResponse(string Token, string Email, Guid TenantId);

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var user = await users.FindByEmailAsync(req.Email);
        if (user == null) return Unauthorized();
        var ok = await signIn.CheckPasswordSignInAsync(user, req.Password, false);
        if (!ok.Succeeded) return Unauthorized();
        var token = await jwt.CreateTokenAsync(user, [], ct);
        return new LoginResponse(token, user.Email ?? "", user.TenantId);
    }
}
