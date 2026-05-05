using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewsDigestBot.Infrastructure.Data;

namespace NewsDigestBot.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly AppDbContext _db;

        public HealthController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Health(CancellationToken ct)
        {
            try
            {
                await _db.Database.CanConnectAsync(ct);
                return Ok(new
                {
                    status = "healthy",
                    database = "connected",
                    timestamp = DateTime.UtcNow
                });
            }
            catch
            {
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    database = "disconnected",
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
