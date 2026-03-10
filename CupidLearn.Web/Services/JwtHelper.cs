using System.IdentityModel.Tokens.Jwt;

namespace CupidLearn.Web.Services;

public static class JwtHelper
{
    public static JwtSecurityToken? ReadToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            return new JwtSecurityTokenHandler().ReadJwtToken(token);
        }
        catch
        {
            return null;
        }
    }

    public static bool IsAdmin(string? token)
    {
        var jwt = ReadToken(token);
        if (jwt == null)
        {
            return false;
        }

        var role = jwt.Claims.FirstOrDefault(x => x.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value
                   ?? jwt.Claims.FirstOrDefault(x => x.Type.EndsWith("/role", StringComparison.OrdinalIgnoreCase) || x.Type == "role")?.Value;

        return string.Equals(role, "ADMIN", StringComparison.OrdinalIgnoreCase);
    }
}
