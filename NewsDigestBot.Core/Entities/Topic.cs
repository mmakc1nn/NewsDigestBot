namespace NewsDigestBot.Core.Entities;

public class Topic
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;        // "Технологии"
    public string Slug { get; set; } = string.Empty;        // "tech"
    public string Description { get; set; } = string.Empty;

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<Article> Articles { get; set; } = new List<Article>();
}