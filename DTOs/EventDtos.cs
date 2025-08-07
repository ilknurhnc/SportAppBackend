namespace SporBackend.DTOs
{
    public class CreateEventDto
    {
        public string Title { get; set; } = string.Empty;
        public string SportType { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public int MaxParticipants { get; set; }
        public string SkillLevel { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class EventDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string SportType { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public int MaxParticipants { get; set; }
        public int CurrentParticipants { get; set; }
        public string SkillLevel { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CreatorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsJoined { get; set; }
    }

    public class EventResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public EventDto? Event { get; set; }
    }
}