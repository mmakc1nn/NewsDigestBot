namespace NewsDigestBot.Core.Entities;

public class Subscription
{
    public int Id { get; set; }
    public long UserId { get; set; }
    public int TopicId { get; set; }
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Topic Topic { get; set; } = null!;
}