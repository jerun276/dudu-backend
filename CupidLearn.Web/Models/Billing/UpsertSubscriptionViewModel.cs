using System.ComponentModel.DataAnnotations;

namespace CupidLearn.Web.Models.Billing;

public class UpsertSubscriptionViewModel
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string Provider { get; set; } = "stripe";

    [Required]
    public string ProviderSubscriptionId { get; set; } = "";

    [Required]
    public string Status { get; set; } = "ACTIVE";

    public DateTimeOffset? CurrentPeriodStart { get; set; }
    public DateTimeOffset? CurrentPeriodEnd { get; set; }
}
