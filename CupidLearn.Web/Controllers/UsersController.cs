using System.Net.Http.Json;
using CupidLearn.Web.Models.Users;
using CupidLearn.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CupidLearn.Web.Controllers;

[Authorize(Roles = "ADMIN")]
public class UsersController(ApiClient apiClient) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] Guid? userId, CancellationToken ct)
    {
        var vm = new UsersIndexViewModel { UserId = userId };

        try
        {
            if (userId.HasValue)
            {
                var client = apiClient.CreateAuthenticatedClient();
                var profile = await client.GetFromJsonAsync<ProfileDto>($"/api/profiles/{userId.Value}", ct);
                if (profile != null)
                {
                    vm.Profile = new UsersIndexViewModel.ProfileVm(
                        profile.UserId,
                        profile.FullName,
                        profile.Email,
                        profile.PhoneNumber,
                        profile.Role,
                        profile.UpdatedAt);
                }
            }
        }
        catch (Exception ex)
        {
            vm.Error = ex.Message;
        }

        return View(vm);
    }

    private sealed record ProfileDto(
        Guid UserId,
        string? FullName,
        string? Email,
        string? PhoneNumber,
        string? Role,
        DateTimeOffset UpdatedAt);
}
