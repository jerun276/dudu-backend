using System.Net.Http.Json;
using CupidLearn.Web.Models.Billing;
using CupidLearn.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CupidLearn.Web.Controllers;

[Authorize(Roles = "ADMIN")]
public class BillingController(ApiClient apiClient) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] Guid? organizationId, [FromQuery] Guid? userId, CancellationToken ct)
    {
        var vm = new BillingIndexViewModel
        {
            OrganizationId = organizationId,
            UserId = userId
        };

        try
        {
            var client = apiClient.CreateAuthenticatedClient();

            if (organizationId.HasValue)
            {
                var seats = await client.GetFromJsonAsync<List<SeatDto>>($"/organizations/{organizationId.Value}/seats", ct) ?? [];
                vm.Seats = seats.Select(x => new BillingIndexViewModel.SeatVm(x.Id, x.OrganizationId, x.UserId, x.Status)).ToList();
            }

            if (userId.HasValue)
            {
                vm.Limits = await client.GetFromJsonAsync<BillingIndexViewModel.LimitsVm>($"/subscriptions/{userId.Value}/limits", ct);

                try
                {
                    var sub = await client.GetFromJsonAsync<SubscriptionDto>($"/subscriptions/{userId.Value}", ct);
                    if (sub != null)
                    {
                        vm.Subscription = new BillingIndexViewModel.SubscriptionVm(sub.Id, sub.UserId, sub.Provider, sub.ProviderSubscriptionId, sub.Status);
                    }
                }
                catch
                {
                    vm.Subscription = null;
                }
            }
        }
        catch (Exception ex)
        {
            vm.Error = ex.Message;
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrganization(CreateOrganizationViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction("Index");
        }

        var client = apiClient.CreateAuthenticatedClient();
        using var resp = await client.PostAsJsonAsync("/organizations", new { name = model.Name, seatCount = model.SeatCount }, ct);
        if (!resp.IsSuccessStatusCode)
        {
            return RedirectToAction("Index");
        }

        var created = await resp.Content.ReadFromJsonAsync<CreateOrgDto>(cancellationToken: ct);
        return RedirectToAction("Index", new { organizationId = created?.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignSeat(AssignRevokeSeatViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction("Index", new { organizationId = model.OrganizationId });
        }

        var client = apiClient.CreateAuthenticatedClient();
        await client.PostAsJsonAsync($"/organizations/{model.OrganizationId}/seats/assign", new { userId = model.UserId }, ct);
        return RedirectToAction("Index", new { organizationId = model.OrganizationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeSeat(AssignRevokeSeatViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction("Index", new { organizationId = model.OrganizationId });
        }

        var client = apiClient.CreateAuthenticatedClient();
        await client.PostAsJsonAsync($"/organizations/{model.OrganizationId}/seats/revoke", new { userId = model.UserId }, ct);
        return RedirectToAction("Index", new { organizationId = model.OrganizationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpsertSubscription(UpsertSubscriptionViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction("Index", new { userId = model.UserId });
        }

        var client = apiClient.CreateAuthenticatedClient();
        await client.PutAsJsonAsync($"/subscriptions/{model.UserId}", new
        {
            provider = model.Provider,
            providerSubscriptionId = model.ProviderSubscriptionId,
            status = model.Status,
            currentPeriodStart = model.CurrentPeriodStart,
            currentPeriodEnd = model.CurrentPeriodEnd
        }, ct);

        return RedirectToAction("Index", new { userId = model.UserId });
    }

    private sealed record CreateOrgDto(Guid Id, string Name);
    private sealed record SeatDto(Guid Id, Guid OrganizationId, Guid? UserId, string Status);
    private sealed record SubscriptionDto(Guid Id, Guid UserId, string Provider, string ProviderSubscriptionId, string Status);
}
