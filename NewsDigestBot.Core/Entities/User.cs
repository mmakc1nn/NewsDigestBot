namespace NewsDigestBot.Core.Entities;

public class User
{
    public long Id { get; set; }          // Telegram Chat ID
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public TimeOnly DigestTime { get; set; } = new TimeOnly(9, 0); // 09:00 по умолчанию
    public int DigestArticleCount { get; set; } = 5;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}