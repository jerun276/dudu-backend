using System.Net.Http.Json;
using System.Security.Claims;
using CupidLearn.Web.Models;
using CupidLearn.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace CupidLearn.Web.Controllers;

public class AccountController(IHttpClientFactory httpClientFactory) : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Admin");
        }

        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var client = httpClientFactory.CreateClient("CupidLearnApi");

        using var resp = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = model.Email,
            password = model.Password
        }, ct);

        if (!resp.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials");
            return View(model);
        }

        var payload = await resp.Content.ReadFromJsonAsync<AuthPayload>(cancellationToken: ct);
        if (payload == null || string.IsNullOrWhiteSpace(payload.AccessToken))
        {
            ModelState.AddModelError(string.Empty, "Login failed");
            return View(model);
        }

        if (!JwtHelper.IsAdmin(payload.AccessToken))
        {
            ModelState.AddModelError(string.Empty, "This account is not an admin");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, payload.UserId.ToString()),
            new(ClaimTypes.Name, payload.Email ?? model.Email),
            new(ClaimTypes.Role, "ADMIN"),
            new("access_token", payload.AccessToken)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        });

        return RedirectToAction("Index", "Admin");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Account");
    }

    private sealed record AuthPayload(Guid UserId, string? Email, string? Role, string AccessToken);
}
