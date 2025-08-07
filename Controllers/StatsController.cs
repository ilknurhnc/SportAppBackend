// Controllers/StatsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SporBackend.Data;

namespace SporBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatsController : ControllerBase
    {
        private readonly SporContext _context;

        public StatsController(SporContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetStats()
        {
            var stats = new
            {
                TotalUsers = await _context.Users.CountAsync(),
                ActiveEvents = await _context.Events.CountAsync(e => e.IsActive && e.EventDate > DateTime.UtcNow),
                TotalEvents = await _context.Events.CountAsync(),
                TotalParticipants = await _context.EventParticipants.CountAsync()
            };

            return Ok(stats);
        }

        [HttpGet("sports")]
        public async Task<ActionResult> GetSportStats()
        {
            var sportStats = await _context.Events
                .Where(e => e.IsActive)
                .GroupBy(e => e.SportType)
                .Select(g => new
                {
                    SportType = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            return Ok(sportStats);
        }
    }
}