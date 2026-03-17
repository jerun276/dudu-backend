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
    public async Task<IActionResult> Index([FromQuery] string? query, [FromQuery] int skip = 0, [FromQuery] int take = 20, [FromQuery] Guid? userId = null, CancellationToken ct = default)
    {
        var vm = new UsersIndexViewModel { Query = query, Skip = skip, Take = take, SelectedUserId = userId };

        try
        {
            var client = apiClient.CreateAuthenticatedClient();

            var search = await client.GetFromJsonAsync<SearchResponse>($"/api/admin/users?query={Uri.EscapeDataString(query ?? string.Empty)}&skip={skip}&take={take}", ct);
            if (search != null)
            {
                vm.Total = search.Total;
                vm.Items = search.Items.Select(x => new UsersIndexViewModel.UserListItemVm(
                    x.UserId,
                    x.Email,
                    x.PhoneNumber,
                    x.Role,
                    x.FullName,
                    x.CreatedAt)).ToList();
            }

            if (userId.HasValue)
            {
                var summary = await client.GetFromJsonAsync<UserSummaryResponse>($"/api/admin/users/{userId.Value}", ct);
                if (summary != null)
                {
                    vm.Summary = new UsersIndexViewModel.UserSummaryVm(
                        summary.UserId,
                        summary.Email,
                        summary.PhoneNumber,
                        summary.Role,
                        summary.FullName,
                        summary.DisplayName,
                        summary.CreatedAt,
                        summary.ProfileUpdatedAt,
                        summary.Subscription == null ? null : new UsersIndexViewModel.SubscriptionVm(
                            summary.Subscription.Id,
                            summary.Subscription.Provider,
                            summary.Subscription.ProviderSubscriptionId,
                            summary.Subscription.Status,
                            summary.Subscription.CurrentPeriodStart,
                            summary.Subscription.CurrentPeriodEnd,
                            summary.Subscription.UpdatedAt),
                        new UsersIndexViewModel.LimitsVm(
                            summary.Limits.Plan,
                            summary.Limits.MaxOrganizations,
                            summary.Limits.MaxSeatsPerOrganization,
                            summary.Limits.MaxChildren),
                        summary.SeatAssignments.Select(s => new UsersIndexViewModel.SeatAssignmentVm(s.SeatId, s.OrganizationId, s.SeatStatus)).ToList(),
                        summary.Children.Select(c => new UsersIndexViewModel.ChildVm(c.Id, c.DisplayName, c.CreatedAt)).ToList());
                }
            }
        }
        catch (Exception ex)
        {
            vm.Error = ex.Message;
        }

        return View(vm);
    }

    private sealed record SearchResponse(int Total, List<SearchItem> Items);
    private sealed record SearchItem(Guid UserId, string Email, string? PhoneNumber, string Role, string? FullName, DateTimeOffset CreatedAt);

    private sealed record LimitsDto(string Plan, int MaxOrganizations, int MaxSeatsPerOrganization, int MaxChildren);
    private sealed record SubscriptionDto(Guid Id, string Provider, string ProviderSubscriptionId, string Status, DateTimeOffset? CurrentPeriodStart, DateTimeOffset? CurrentPeriodEnd, DateTimeOffset UpdatedAt);
    private sealed record SeatAssignmentDto(Guid SeatId, Guid OrganizationId, string SeatStatus);
    private sealed record ChildDto(Guid Id, string DisplayName, DateTimeOffset CreatedAt);

    private sealed record UserSummaryResponse(
        Guid UserId,
        string Email,
        string? PhoneNumber,
        string Role,
        string? FullName,
        string? DisplayName,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ProfileUpdatedAt,
        SubscriptionDto? Subscription,
        LimitsDto Limits,
        List<SeatAssignmentDto> SeatAssignments,
        List<ChildDto> Children);
}
