using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewsDigestBot.Infrastructure.Data;
using Telegram.Bot;

namespace NewsDigestBot.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ITelegramBotClient _bot;

        public HealthController(AppDbContext db, ITelegramBotClient bot)
        {
            _db = db;
            _bot = bot;
        }

        [HttpGet]
        public async Task<IActionResult> Health(CancellationToken ct)
        {

            var dbOk = false;
            var botOk = false;

            try
            {
                await _db.Database.CanConnectAsync(ct);
                dbOk = true;
            }
            catch { }

            try
            {
                await _bot.GetMe(ct);
                botOk = true;
            }
            catch { }

            var status = dbOk && botOk ? "healthy" : "unhealthy";
            var code = dbOk && botOk ? 200 : 503;

            return StatusCode(code, new
            {
                status,
                checks = new
                {
                    database = dbOk ? "connected" : "disconnected",
                    telegram = botOk ? "connected" : "disconnected"
                },
                timestamp = DateTime.UtcNow
            });

        }
    }
}
