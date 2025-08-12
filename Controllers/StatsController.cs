// Controllers/StatsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporBackend.Data;
using System.Security.Claims;

namespace SporBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatsController : ControllerBase
    {
        private readonly SporContext _context;
        private readonly ILogger<StatsController> _logger;

        public StatsController(SporContext context, ILogger<StatsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetStats()
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting general stats");
                return StatusCode(500, new { Success = false, Message = "İstatistikler yüklenirken hata oluştu" });
            }
        }

        [HttpGet("sports")]
        public async Task<ActionResult> GetSportStats()
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sport stats");
                return StatusCode(500, new { Success = false, Message = "Spor istatistikleri yüklenirken hata oluştu" });
            }
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<ActionResult> GetUserStats()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { Success = false, Message = "Giriş yapmalısınız" });
                }

                _logger.LogInformation($"Getting user stats for user: {userId}");

                // Kullanıcının katıldığı etkinlikler (aktif)
                var joinedEvents = await _context.EventParticipants
                    .Where(ep => ep.UserId == userId.Value)
                    .CountAsync();

                // Kullanıcının oluşturduğu etkinlikler
                var createdEvents = await _context.Events
                    .Where(e => e.CreatorId == userId.Value)
                    .CountAsync();

                // Tamamlanan etkinlikler (geçmiş tarihli ve kullanıcının katıldığı)
                var completedEvents = await _context.EventParticipants
                    .Where(ep => ep.UserId == userId.Value && ep.Event.EventDate < DateTime.UtcNow)
                    .CountAsync();

                // Spor arkadaşları (aynı etkinliklere katılan farklı kullanıcılar)
                var sportFriends = await _context.EventParticipants
                    .Where(ep1 => ep1.UserId == userId.Value)
                    .Join(_context.EventParticipants,
                        ep1 => ep1.EventId,
                        ep2 => ep2.EventId,
                        (ep1, ep2) => ep2.UserId)
                    .Where(friendId => friendId != userId.Value)
                    .Distinct()
                    .CountAsync();

                var userStats = new
                {
                    JoinedEvents = joinedEvents,
                    CreatedEvents = createdEvents,
                    CompletedEvents = completedEvents,
                    SportFriends = sportFriends
                };

                _logger.LogInformation($"User stats: {System.Text.Json.JsonSerializer.Serialize(userStats)}");

                return Ok(userStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user stats");
                return StatusCode(500, new { Success = false, Message = "Kullanıcı istatistikleri yüklenirken hata oluştu" });
            }
        }

        private int? GetCurrentUserId()
        {
            try
            {
                var userIdClaim = User.FindFirst("userId")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    return userId;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user ID");
                return null;
            }
        }
    }
}
