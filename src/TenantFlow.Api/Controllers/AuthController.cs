using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenantFlow.Api.Models;
using TenantFlow.Api.Services;

namespace TenantFlow.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private const string AuthCookieName = "tenantflow_auth";
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        if (response is null)
        {
            return Unauthorized();
        }

        AppendAuthCookie(response.Token, response.ExpiresAtUtc);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("demo")]
    public async Task<ActionResult<AuthResponse>> Demo([FromBody] DemoLoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.DemoAsync(request, cancellationToken);
        AppendAuthCookie(response.Token, response.ExpiresAtUtc);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public ActionResult Logout()
    {
        Response.Cookies.Delete(AuthCookieName, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = Request.IsHttps,
            Path = "/"
        });

        return NoContent();
    }

    private void AppendAuthCookie(string token, DateTime expiresAtUtc)
    {
        Response.Cookies.Append(AuthCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = Request.IsHttps,
            Expires = expiresAtUtc,
            IsEssential = true,
            Path = "/"
        });
    }
}
