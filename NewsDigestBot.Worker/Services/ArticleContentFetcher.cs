using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsDigestBot.Worker.Services
{
    public class ArticleContentFetcher
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ArticleContentFetcher> _logger;

        public ArticleContentFetcher(HttpClient httpClient, ILogger<ArticleContentFetcher> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string?> FetchContentAsync(string url, CancellationToken ct = default)
        {
            try
            {
                var html = await _httpClient.GetStringAsync(url, ct);
                _logger.LogInformation("HTML length for {Url}: {Length}", url, html.Length);



                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                foreach (var node in doc.DocumentNode
                    .SelectNodes("//script|//style|//nav|//header|//footer|//aside|//form")
                    ?? Enumerable.Empty<HtmlNode>())
                {
                    node.Remove();
                }

                var contentNode =
                    doc.DocumentNode.SelectSingleNode("//article") ??
                    doc.DocumentNode.SelectSingleNode("//*[contains(@class,'content')]") ??
                    doc.DocumentNode.SelectSingleNode("//*[contains(@class,'article')]") ??
                    doc.DocumentNode.SelectSingleNode("//*[contains(@class,'text')]") ??
                    doc.DocumentNode.SelectSingleNode("//main") ??
                    doc.DocumentNode.SelectSingleNode("//body");

                if (contentNode is null) return null;

                var text = contentNode.InnerText;
                text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();

                _logger.LogInformation("Content extracted for {Url}: {Length} chars, node: {Node}",
                    url, text.Length, contentNode.Name + " " + contentNode.GetAttributeValue("class", ""));

                return text.Length > 3000 ? text[..3000] : text;


            }
            catch (Exception ex)
            {
                _logger.LogWarning("Не удалось скачать контент {Url}: {Error}", url, ex.Message);
                return null;
            }
        }
    }
}
