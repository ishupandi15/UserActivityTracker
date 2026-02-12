using System.Collections.Generic;

namespace ActivityTracker.Models
{
    /// <summary>
    /// Represents an application user.
    /// IDs are BIGINT-like (long). Email uniqueness is enforced via DB unique index.
    /// </summary>
    public class User
    {
        public long ID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public ICollection<UserActivity> Activities { get; set; } = new List<UserActivity>();
    }
}