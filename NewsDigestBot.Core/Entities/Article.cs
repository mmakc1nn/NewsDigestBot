namespace NewsDigestBot.Core.Entities;

public class Article
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? OriginalContent { get; set; }
    public string? Summary { get; set; }           // AI резюме 2-3 предложения
    public bool IsSummarized { get; set; } = false;
    public DateTime PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = string.Empty;     // "BBC", "Hacker News"...

    public int TopicId { get; set; }
    public Topic Topic { get; set; } = null!;
}