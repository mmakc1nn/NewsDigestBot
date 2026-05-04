using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsDigestBot.Worker.Configuration
{
    public static class RssSources
    {
        public static readonly List<RssSource> Sources = new()
        {
            new("https://lenta.ru/rss/news",          "Lenta.ru",  "world"),
            new("https://lenta.ru/rss/news/science",  "Lenta.ru",  "science"),
            new("https://lenta.ru/rss/news/sport",    "Lenta.ru",  "sport"),
            new("https://lenta.ru/rss/news/economy",  "Lenta.ru",  "business"),
            new("https://habr.com/ru/rss/articles/",  "Habr",      "tech"),
            new("https://meduza.io/rss/all",          "Meduza",    "world"),
        };
    }

    public record RssSource(string Url, string Name, string TopicSlug);
}
