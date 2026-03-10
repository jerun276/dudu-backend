using System.ComponentModel.DataAnnotations;

namespace CupidLearn.Web.Models.Billing;

public class AssignRevokeSeatViewModel
{
    [Required]
    public Guid OrganizationId { get; set; }

    [Required]
    public Guid UserId { get; set; }
}
