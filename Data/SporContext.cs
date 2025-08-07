// Data/SporContext.cs
using Microsoft.EntityFrameworkCore;
using SporBackend.Models;

namespace SporBackend.Data
{
    public class SporContext : DbContext
    {
        public SporContext(DbContextOptions<SporContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventParticipant> EventParticipants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
            });

            // Event configuration
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.SportType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Location).IsRequired().HasMaxLength(200);
                entity.Property(e => e.SkillLevel).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(1000);

                entity.HasOne(e => e.Creator)
                    .WithMany(u => u.CreatedEvents)
                    .HasForeignKey(e => e.CreatorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // EventParticipant configuration
            modelBuilder.Entity<EventParticipant>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(ep => ep.Event)
                    .WithMany(e => e.Participants)
                    .HasForeignKey(ep => ep.EventId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ep => ep.User)
                    .WithMany(u => u.ParticipatedEvents)
                    .HasForeignKey(ep => ep.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint: Bir kullanıcı aynı etkinliğe sadece bir kez katılabilir
                entity.HasIndex(ep => new { ep.EventId, ep.UserId }).IsUnique();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}