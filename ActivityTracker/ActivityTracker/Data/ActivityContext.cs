using Microsoft.EntityFrameworkCore;
using ActivityTracker.Models;

namespace ActivityTracker
{
    public class ActivityContext : DbContext
    {
        public ActivityContext(DbContextOptions<ActivityContext> options)
            : base(options)
        {
            // Optionally set no-tracking globally for read-heavy scenarios:
            // ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserActivity> UserActivities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Table names
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<UserActivity>().ToTable("UserActivities");

            // Primary keys (explicit) and identity generation
            modelBuilder.Entity<User>().HasKey(u => u.ID);
            modelBuilder.Entity<User>().Property(u => u.ID).ValueGeneratedOnAdd();

            modelBuilder.Entity<UserActivity>().HasKey(a => a.ID);
            modelBuilder.Entity<UserActivity>().Property(a => a.ID).ValueGeneratedOnAdd();

            // User properties
            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(320);

            // Unique constraint on email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("UX_Users_Email");

            // UserActivity properties
            modelBuilder.Entity<UserActivity>()
                .Property(a => a.ActivityType)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<UserActivity>()
                .Property(a => a.SessionID)
                .IsRequired()
                .HasMaxLength(100);

            // Persist timestamps as datetimeoffset in the DB
            modelBuilder.Entity<UserActivity>()
                .Property(a => a.ActivityTimestamp)
                .HasColumnType("datetimeoffset")
                .IsRequired();

            // Indexes for performance on common query patterns
            modelBuilder.Entity<UserActivity>()
                .HasIndex(a => a.ActivityTimestamp)
                .HasDatabaseName("IX_UserActivities_ActivityTimestamp");

            modelBuilder.Entity<UserActivity>()
                .HasIndex(a => a.UserID)
                .HasDatabaseName("IX_UserActivities_UserID");

            modelBuilder.Entity<UserActivity>()
                .HasIndex(a => a.SessionID)
                .HasDatabaseName("IX_UserActivities_SessionID");

            // Composite indexes that help grouped queries (session durations, time-range per user)
            modelBuilder.Entity<UserActivity>()
                .HasIndex(a => new { a.UserID, a.ActivityTimestamp })
                .HasDatabaseName("IX_UserActivities_UserID_ActivityTimestamp");

            modelBuilder.Entity<UserActivity>()
                .HasIndex(a => new { a.SessionID, a.ActivityTimestamp })
                .HasDatabaseName("IX_UserActivities_SessionID_ActivityTimestamp");

            // Foreign key relationship (referential integrity)
            modelBuilder.Entity<UserActivity>()
                .HasOne<User>()
                .WithMany(u => u.Activities)
                .HasForeignKey(a => a.UserID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_UserActivities_Users_UserID");
        }
    }
}