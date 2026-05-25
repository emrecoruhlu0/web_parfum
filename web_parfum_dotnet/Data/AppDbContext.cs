using Microsoft.EntityFrameworkCore;
using WebParfum.Models;

namespace WebParfum.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Perfume> Perfumes { get; set; }
    public DbSet<Collection> Collections { get; set; }
    public DbSet<DailyLog> DailyLogs { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Follow> Follows { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Community> Communities { get; set; }
    public DbSet<CommunityMember> CommunityMembers { get; set; }
    public DbSet<CommunityPost> CommunityPosts { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Unique constraints
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<Review>()
            .HasIndex(r => new { r.UserId, r.PerfumeId }).IsUnique();

        modelBuilder.Entity<Like>()
            .HasIndex(l => new { l.UserId, l.PerfumeId }).IsUnique();

        // Bir kullanıcı aynı parfümü hem owned hem wishlist hem tried işaretleyebilir,
        // ama aynı status iki kez olamaz.
        modelBuilder.Entity<Collection>()
            .HasIndex(c => new { c.UserId, c.PerfumeId, c.Status }).IsUnique();

        modelBuilder.Entity<Follow>()
            .HasIndex(f => new { f.FollowerId, f.FollowingId }).IsUnique();

        modelBuilder.Entity<CommunityMember>()
            .HasIndex(cm => new { cm.CommunityId, cm.UserId }).IsUnique();

        // Follow — iki ayrı User ilişkisi (circular ref önlemi)
        modelBuilder.Entity<Follow>()
            .HasOne(f => f.Follower)
            .WithMany(u => u.Following)
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Follow>()
            .HasOne(f => f.Following)
            .WithMany(u => u.Followers)
            .HasForeignKey(f => f.FollowingId)
            .OnDelete(DeleteBehavior.Restrict);

        // Notification — iki ayrı User ilişkisi
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Recipient)
            .WithMany(u => u.ReceivedNotifications)
            .HasForeignKey(n => n.RecipientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Actor)
            .WithMany(u => u.SentNotifications)
            .HasForeignKey(n => n.ActorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Message — iki ayrı User ilişkisi
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Recipient)
            .WithMany(u => u.ReceivedMessages)
            .HasForeignKey(m => m.RecipientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Community owner
        modelBuilder.Entity<Community>()
            .HasOne(c => c.Owner)
            .WithMany()
            .HasForeignKey(c => c.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
