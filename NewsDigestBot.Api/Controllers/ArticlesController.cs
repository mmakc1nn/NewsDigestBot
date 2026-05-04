using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsDigestBot.Infrastructure.Data;

namespace NewsDigestBot.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArticlesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ArticlesController(AppDbContext db)
        {
            _db = db;
        }

        // GET /api/articles?topic=tech&limit=10
        [HttpGet]
        public async Task<IActionResult> GetArticles(
            [FromQuery] string? topic,
            [FromQuery] int limit = 20,
            CancellationToken ct = default)
        {
            var query = _db.Articles
                .Include(a => a.Topic)
                .AsQueryable();

            if (!string.IsNullOrEmpty(topic))
                query = query.Where(a => a.Topic.Slug == topic);

            var articles = await query
                .OrderByDescending(a => a.PublishedAt)
                .Take(Math.Min(limit, 100))
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.Url,
                    a.Summary,
                    a.Source,
                    a.PublishedAt,
                    Topic = a.Topic.Name
                })
                .ToListAsync(ct);

            return Ok(articles);
        }

        // GET /api/articles/search?q=ai
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string q,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Укажи поисковый запрос");

            var results = await _db.Articles
                .Include(a => a.Topic)
                .Where(a => EF.Functions.ILike(a.Title, $"%{q}%"))
                .OrderByDescending(a => a.PublishedAt)
                .Take(20)
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.Url,
                    a.Summary,
                    a.Source,
                    Topic = a.Topic.Name
                })
                .ToListAsync(ct);

            return Ok(results);
        }
    }
}
