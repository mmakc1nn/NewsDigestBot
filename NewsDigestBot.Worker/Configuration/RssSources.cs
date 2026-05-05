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
            new("https://habr.com/ru/rss/articles/",                              "Habr",     "tech"),
            new("https://meduza.io/rss/all",                                      "Meduza",   "world"),
            new("https://feeds.bbci.co.uk/news/technology/rss.xml",               "BBC",      "tech"),
            new("https://feeds.bbci.co.uk/news/science_and_environment/rss.xml",  "BBC",      "science"),
            new("https://feeds.bbci.co.uk/news/business/rss.xml",                 "BBC",      "business"),
            new("https://feeds.bbci.co.uk/news/world/rss.xml",                    "BBC",      "world"),
        };
    }

    public record RssSource(string Url, string Name, string TopicSlug);
}
