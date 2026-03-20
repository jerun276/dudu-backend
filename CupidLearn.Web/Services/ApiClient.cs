using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace CupidLearn.Web.Services;

public class ApiClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
{
    public HttpClient CreateAuthenticatedClient()
    {
        var client = httpClientFactory.CreateClient("CupidLearnApi");

        var ctx = httpContextAccessor.HttpContext;
        if (ctx?.User?.Identity?.IsAuthenticated == true)
        {
            var token = ctx.User.FindFirst("access_token")?.Value;
            if (string.IsNullOrWhiteSpace(token))
            {
                token = ctx.GetTokenAsync("access_token").GetAwaiter().GetResult();
            }
            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        return client;
    }

    public static string? GetRole(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Role)
               ?? user.FindFirstValue("role")
               ?? user.FindFirstValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
    }
}
