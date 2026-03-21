using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CupidLearn.Web.Services;

/// <summary>
/// Intercepts 401 Unauthorized responses from the API, attempts a token refresh,
/// re-signs in the user with fresh tokens, and retries the original request.
/// </summary>
public class TokenRefreshHandler(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Send the original request
        var response = await base.SendAsync(request, cancellationToken);

        // Only attempt refresh on 401 Unauthorized
        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        var ctx = httpContextAccessor.HttpContext;
        if (ctx == null)
        {
            return response;
        }

        // Grab the refresh token from the current user's claims
        var refreshToken = ctx.User.FindFirst(SessionKeys.RefreshToken)?.Value;
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            // No refresh token — force re-login
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return response;
        }

        // Call the refresh endpoint
        var refreshClient = httpClientFactory.CreateClient("CupidLearnApi");
        AuthPayload? payload = null;

        try
        {
            using var refreshResp = await refreshClient.PostAsJsonAsync("/api/auth/refresh", new
            {
                refreshToken
            }, cancellationToken);

            if (!refreshResp.IsSuccessStatusCode)
            {
                // Refresh failed — force re-login
                await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return response;
            }

            payload = await refreshResp.Content.ReadFromJsonAsync<AuthPayload>(cancellationToken: cancellationToken);
        }
        catch
        {
            return response;
        }

        if (payload == null || string.IsNullOrWhiteSpace(payload.AccessToken))
        {
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return response;
        }

        // Re-issue the auth cookie with the fresh tokens
        var existingClaims = ctx.User.Claims
            .Where(c => c.Type != SessionKeys.AccessToken && c.Type != SessionKeys.RefreshToken)
            .ToList();

        var claims = new List<Claim>(existingClaims)
        {
            new(SessionKeys.AccessToken, payload.AccessToken)
        };

        if (!string.IsNullOrWhiteSpace(payload.RefreshToken))
        {
            claims.Add(new Claim(SessionKeys.RefreshToken, payload.RefreshToken));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        });

        // Dispose the original 401 response before retrying
        response.Dispose();

        // Clone the original request (HttpRequestMessage can only be sent once)
        var retryRequest = await CloneRequestAsync(request, payload.AccessToken, cancellationToken);
        return await base.SendAsync(retryRequest, cancellationToken);
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage original, string newAccessToken, CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri);

        // Copy headers (except Authorization which we'll replace)
        foreach (var header in original.Headers)
        {
            if (!string.Equals(header.Key, "Authorization", StringComparison.OrdinalIgnoreCase))
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        clone.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);

        // Copy content if present
        if (original.Content != null)
        {
            var ms = new MemoryStream();
            await original.Content.CopyToAsync(ms, cancellationToken);
            ms.Seek(0, SeekOrigin.Begin);
            clone.Content = new StreamContent(ms);

            foreach (var header in original.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return clone;
    }

    private sealed record AuthPayload(Guid UserId, string? Email, string? Role, string AccessToken, string? RefreshToken);
}
