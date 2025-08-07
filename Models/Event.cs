namespace SporBackend.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string SportType { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public int MaxParticipants { get; set; }
        public string SkillLevel { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CreatorId { get; set; }
        public User Creator { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public List<EventParticipant> Participants { get; set; } = new();
    }
}
