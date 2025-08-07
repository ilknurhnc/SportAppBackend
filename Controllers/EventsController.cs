// Controllers/EventsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SporBackend.DTOs;
using SporBackend.Services;

namespace SporBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet]
        public async Task<ActionResult<List<EventDto>>> GetAllEvents()
        {
            var userId = GetCurrentUserId();
            var events = await _eventService.GetAllEventsAsync(userId);
            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EventDto>> GetEvent(int id)
        {
            var userId = GetCurrentUserId();
            var eventDto = await _eventService.GetEventByIdAsync(id, userId);
            
            if (eventDto == null)
            {
                return NotFound(new { Success = false, Message = "Etkinlik bulunamadı" });
            }

            return Ok(eventDto);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<EventResponseDto>> CreateEvent([FromBody] CreateEventDto createEventDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { Success = false, Message = "Giriş yapmalısınız" });
            }

            var result = await _eventService.CreateEventAsync(createEventDto, userId.Value);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("{id}/join")]
        [Authorize]
        public async Task<ActionResult<EventResponseDto>> JoinEvent(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { Success = false, Message = "Giriş yapmalısınız" });
            }

            var result = await _eventService.JoinEventAsync(id, userId.Value);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("{id}/leave")]
        [Authorize]
        public async Task<ActionResult<EventResponseDto>> LeaveEvent(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { Success = false, Message = "Giriş yapmalısınız" });
            }

            var result = await _eventService.LeaveEventAsync(id, userId.Value);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("my-events")]
        [Authorize]
        public async Task<ActionResult<List<EventDto>>> GetMyEvents()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { Success = false, Message = "Giriş yapmalısınız" });
            }

            var events = await _eventService.GetUserEventsAsync(userId.Value);
            return Ok(events);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}
