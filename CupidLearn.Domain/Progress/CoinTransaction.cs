namespace CupidLearn.Domain.Progress;

public class CoinTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid ChildId { get; set; }
    public int Amount { get; set; }
    public string Reason { get; set; } = "";
    public Guid? ReferenceId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
