namespace SporBackend.Models
{
    public class EventParticipant
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public Event Event { get; set; } = null!;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
