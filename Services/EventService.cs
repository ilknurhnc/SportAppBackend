// Services/EventService.cs
using Microsoft.EntityFrameworkCore;
using SporBackend.Data;
using SporBackend.DTOs;
using SporBackend.Models;

namespace SporBackend.Services
{
    public class EventService : IEventService
    {
        private readonly SporContext _context;

        public EventService(SporContext context)
        {
            _context = context;
        }

        public async Task<List<EventDto>> GetAllEventsAsync(int? userId = null)
        {
            var events = await _context.Events
                .Include(e => e.Creator)
                .Include(e => e.Participants)
                .Where(e => e.IsActive && e.EventDate > DateTime.UtcNow)
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            return events.Select(e => new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                SportType = e.SportType,
                Location = e.Location,
                EventDate = e.EventDate,
                MaxParticipants = e.MaxParticipants,
                CurrentParticipants = e.Participants.Count,
                SkillLevel = e.SkillLevel,
                Description = e.Description,
                CreatorName = e.Creator.Name,
                CreatedAt = e.CreatedAt,
                IsJoined = userId.HasValue && e.Participants.Any(p => p.UserId == userId.Value)
            }).ToList();
        }

        public async Task<EventDto?> GetEventByIdAsync(int eventId, int? userId = null)
        {
            var eventEntity = await _context.Events
                .Include(e => e.Creator)
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventEntity == null) return null;

            return new EventDto
            {
                Id = eventEntity.Id,
                Title = eventEntity.Title,
                SportType = eventEntity.SportType,
                Location = eventEntity.Location,
                EventDate = eventEntity.EventDate,
                MaxParticipants = eventEntity.MaxParticipants,
                CurrentParticipants = eventEntity.Participants.Count,
                SkillLevel = eventEntity.SkillLevel,
                Description = eventEntity.Description,
                CreatorName = eventEntity.Creator.Name,
                CreatedAt = eventEntity.CreatedAt,
                IsJoined = userId.HasValue && eventEntity.Participants.Any(p => p.UserId == userId.Value)
            };
        }

        public async Task<EventResponseDto> CreateEventAsync(CreateEventDto createEventDto, int userId)
        {
            try
            {
                var eventEntity = new Event
                {
                    Title = createEventDto.Title,
                    SportType = createEventDto.SportType,
                    Location = createEventDto.Location,
                    EventDate = createEventDto.EventDate,
                    MaxParticipants = createEventDto.MaxParticipants,
                    SkillLevel = createEventDto.SkillLevel,
                    Description = createEventDto.Description,
                    CreatorId = userId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Events.Add(eventEntity);
                await _context.SaveChangesAsync();

                // Creator'ı otomatik olarak katılımcı yap
                var participant = new EventParticipant
                {
                    EventId = eventEntity.Id,
                    UserId = userId,
                    JoinedAt = DateTime.UtcNow
                };

                _context.EventParticipants.Add(participant);
                await _context.SaveChangesAsync();

                return new EventResponseDto
                {
                    Success = true,
                    Message = "Etkinlik başarıyla oluşturuldu"
                };
            }
            catch (Exception ex)
            {
                return new EventResponseDto
                {
                    Success = false,
                    Message = "Etkinlik oluşturulurken hata oluştu"
                };
            }
        }

        public async Task<EventResponseDto> JoinEventAsync(int eventId, int userId)
        {
            try
            {
                var eventEntity = await _context.Events
                    .Include(e => e.Participants)
                    .FirstOrDefaultAsync(e => e.Id == eventId);

                if (eventEntity == null)
                {
                    return new EventResponseDto
                    {
                        Success = false,
                        Message = "Etkinlik bulunamadı"
                    };
                }

                if (eventEntity.Participants.Count >= eventEntity.MaxParticipants)
                {
                    return new EventResponseDto
                    {
                        Success = false,
                        Message = "Etkinlik dolu"
                    };
                }

                if (eventEntity.Participants.Any(p => p.UserId == userId))
                {
                    return new EventResponseDto
                    {
                        Success = false,
                        Message = "Zaten bu etkinliğe katıldınız"
                    };
                }

                var participant = new EventParticipant
                {
                    EventId = eventId,
                    UserId = userId,
                    JoinedAt = DateTime.UtcNow
                };

                _context.EventParticipants.Add(participant);
                await _context.SaveChangesAsync();

                return new EventResponseDto
                {
                    Success = true,
                    Message = "Etkinliğe başarıyla katıldınız"
                };
            }
            catch (Exception ex)
            {
                return new EventResponseDto
                {
                    Success = false,
                    Message = "Etkinliğe katılırken hata oluştu"
                };
            }
        }

        public async Task<EventResponseDto> LeaveEventAsync(int eventId, int userId)
        {
            try
            {
                var participant = await _context.EventParticipants
                    .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId);

                if (participant == null)
                {
                    return new EventResponseDto
                    {
                        Success = false,
                        Message = "Bu etkinliğe katılmamışsınız"
                    };
                }

                _context.EventParticipants.Remove(participant);
                await _context.SaveChangesAsync();

                return new EventResponseDto
                {
                    Success = true,
                    Message = "Etkinlikten başarıyla ayrıldınız"
                };
            }
            catch (Exception ex)
            {
                return new EventResponseDto
                {
                    Success = false,
                    Message = "Etkinlikten ayrılırken hata oluştu"
                };
            }
        }

        public async Task<List<EventDto>> GetUserEventsAsync(int userId)
        {
            var events = await _context.Events
                .Include(e => e.Creator)
                .Include(e => e.Participants)
                .Where(e => e.Participants.Any(p => p.UserId == userId))
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            return events.Select(e => new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                SportType = e.SportType,
                Location = e.Location,
                EventDate = e.EventDate,
                MaxParticipants = e.MaxParticipants,
                CurrentParticipants = e.Participants.Count,
                SkillLevel = e.SkillLevel,
                Description = e.Description,
                CreatorName = e.Creator.Name,
                CreatedAt = e.CreatedAt,
                IsJoined = true
            }).ToList();
        }
    }
}