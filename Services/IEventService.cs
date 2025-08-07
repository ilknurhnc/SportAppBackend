using SporBackend.DTOs;

namespace SporBackend.Services
{
    public interface IEventService
    {
        Task<List<EventDto>> GetAllEventsAsync(int? userId = null);
        Task<EventDto?> GetEventByIdAsync(int eventId, int? userId = null);
        Task<EventResponseDto> CreateEventAsync(CreateEventDto createEventDto, int userId);
        Task<EventResponseDto> JoinEventAsync(int eventId, int userId);
        Task<EventResponseDto> LeaveEventAsync(int eventId, int userId);
        Task<List<EventDto>> GetUserEventsAsync(int userId);
    }
}