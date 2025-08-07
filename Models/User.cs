namespace SporBackend.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<Event> CreatedEvents { get; set; } = new();
        public List<EventParticipant> ParticipatedEvents { get; set; } = new();
    }
}