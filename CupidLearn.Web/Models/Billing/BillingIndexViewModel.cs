namespace CupidLearn.Web.Models.Billing;

public class BillingIndexViewModel
{
    public string? Error { get; set; }

    public Guid? OrganizationId { get; set; }
    public List<SeatVm> Seats { get; set; } = [];

    public Guid? UserId { get; set; }
    public SubscriptionVm? Subscription { get; set; }
    public LimitsVm? Limits { get; set; }

    public record SeatVm(Guid Id, Guid OrganizationId, Guid? UserId, string Status);

    public record SubscriptionVm(Guid Id, Guid UserId, string Provider, string ProviderSubscriptionId, string Status);

    public record LimitsVm(string Plan, int MaxOrganizations, int MaxSeatsPerOrganization, int MaxChildren);
}
