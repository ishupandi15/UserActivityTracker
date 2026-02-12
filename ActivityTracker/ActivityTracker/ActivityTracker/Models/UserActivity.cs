using System;

namespace ActivityTracker.Models
{
    /// <summary>
    /// A single user activity (page view, click, etc).
    /// ActivityTimestamp is DateTimeOffset and should be set to UTC (DateTimeOffset.UtcNow) when logging.
    /// Metadata is free-form JSON stored as a string.
    /// </summary>
    public class UserActivity
    {
        public long ID { get; set; }
        public long UserID { get; set; }

  
        public string ActivityType { get; set; } = string.Empty;

        // Use DateTimeOffset for reliable timezone/UTC handling and to match DB mapping.
        public DateTimeOffset ActivityTimestamp { get; set; }

        public string SessionID { get; set; } = string.Empty;

        // Free-form JSON (or key:value); ensure DB column supports nvarchar(max)
        public string Metadata { get; set; } = string.Empty;

        // Navigation property
        public User? User { get; set; }
    }
}