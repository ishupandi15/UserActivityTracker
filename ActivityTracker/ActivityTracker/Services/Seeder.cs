using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ActivityTracker.Models;

namespace ActivityTracker.Services
{
    public static class Seeder
    {
        /// <summary>
        /// Seed bulk users + activities. Generates multiple sessions per user and multiple activities per session
        /// so that average-per-session and session-duration reports are meaningful.
        /// </summary>
        /// <param name="db">ActivityContext (must be configured)</param>
        /// <param name="numUsers">Number of users to create</param>
        /// <param name="activitiesPerUser">Total activities to create per user (distributed across sessions)</param>
        public static async Task SeedBulk(ActivityContext db, int numUsers, int activitiesPerUser)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (numUsers <= 0) return;
            if (activitiesPerUser <= 0) activitiesPerUser = 1;

            Console.WriteLine($"Seeding {numUsers} users x {activitiesPerUser} activities...");

            // 1) create users via EF to obtain IDs
            var users = new List<User>(numUsers);
            for (int i = 0; i < numUsers; i++)
            {
                var idSuffix = Guid.NewGuid().ToString("N").Substring(0, 8);
                users.Add(new User
                {
                    Username = $"seed_user_{idSuffix}",
                    Email = $"seed_{idSuffix}@example.com"
                });
            }

            await db.Users.AddRangeAsync(users);
            await db.SaveChangesAsync();

            var userIds = users.Select(u => u.ID).ToArray();

            // 2) Prepare DataTable for SqlBulkCopy
            var table = new DataTable();
            table.Columns.Add("UserID", typeof(long));
            table.Columns.Add("ActivityType", typeof(string));
            table.Columns.Add("ActivityTimestamp", typeof(DateTimeOffset));
            table.Columns.Add("SessionID", typeof(string));
            table.Columns.Add("Metadata", typeof(string));

            var rnd = new Random();
            var activityTypes = new[] { "PageView", "Click", "FormSubmit", "Navigation", "Purchase" };

            // Heuristics: create a small number of sessions per user so each session has multiple activities
            for (int u = 0; u < userIds.Length; u++)
            {
                var uid = userIds[u];

                int sessionsPerUser = Math.Max(1, activitiesPerUser / 4);
                sessionsPerUser = Math.Min(sessionsPerUser, activitiesPerUser);

                int baseActivitiesPerSession = activitiesPerUser / sessionsPerUser;
                int remainder = activitiesPerUser % sessionsPerUser;

                for (int s = 0; s < sessionsPerUser; s++)
                {
                    var sessionId = Guid.NewGuid().ToString();
                    int activitiesInSession = baseActivitiesPerSession + (s < remainder ? 1 : 0);

                    var sessionBase = DateTimeOffset.UtcNow.AddMinutes(-rnd.Next(0, 60 * 24 * 30));

                    for (int a = 0; a < activitiesInSession; a++)
                    {
                        var activityType = activityTypes[rnd.Next(activityTypes.Length)];
                        var timestamp = sessionBase.AddMinutes(rnd.Next(0, 120)).AddSeconds(rnd.Next(0, 60));
                        var metadata = "{\"sample\":true}";

                        table.Rows.Add(uid, activityType, timestamp, sessionId, metadata);

                        if (table.Rows.Count >= 50000)
                        {
                            await WriteTableToServerAsync(db.Database.GetDbConnection().ConnectionString, table);
                            table.Clear();
                        }
                    }
                }
            }

            if (table.Rows.Count > 0)
            {
                await WriteTableToServerAsync(db.Database.GetDbConnection().ConnectionString, table);
                table.Clear();
            }

            Console.WriteLine("Seeding finished.");
        }

        private static async Task WriteTableToServerAsync(string connectionString, DataTable table)
        {
            if (table == null || table.Rows.Count == 0) return;

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            // SqlBulkCopy does not implement IAsyncDisposable; use regular using
            using var bulk = new SqlBulkCopy(conn, SqlBulkCopyOptions.TableLock, null)
            {
                DestinationTableName = "dbo.UserActivities",
                BulkCopyTimeout = 0 // infinite timeout; adjust as required
            };

            // Column mappings must match table schema
            bulk.ColumnMappings.Add("UserID", "UserID");
            bulk.ColumnMappings.Add("ActivityType", "ActivityType");
            bulk.ColumnMappings.Add("ActivityTimestamp", "ActivityTimestamp");
            bulk.ColumnMappings.Add("SessionID", "SessionID");
            bulk.ColumnMappings.Add("Metadata", "Metadata");

            // Use the async API to avoid blocking threads
            await bulk.WriteToServerAsync(table);

            await conn.CloseAsync();
        }
    }
}