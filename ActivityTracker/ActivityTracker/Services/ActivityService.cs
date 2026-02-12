using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ActivityTracker.Models;

namespace ActivityTracker.Services
{
    public class ActivityService : IAsyncDisposable
    {
        private readonly ActivityContext _db;

        private readonly Channel<UserActivity> _channel;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Task _backgroundWriter;

        private readonly int _batchSize = 500;
        private readonly TimeSpan _flushInterval = TimeSpan.FromMilliseconds(250);

        public ActivityService(ActivityContext db)
        {
            _db = db;

            var options = new BoundedChannelOptions(10000)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait
            };

            _channel = Channel.CreateBounded<UserActivity>(options);
            _backgroundWriter = Task.Run(() => ProcessQueueAsync(_cts.Token));
        }

        // ---------------- LOGGING ----------------

        // metadata is nullable here; we normalize it when creating the entity
        public async Task<UserActivity> LogUserActivity(
            long userId,
            string activityType,
            string sessionId,
            string? metadata = null)
        {
            var activity = new UserActivity
            {
                UserID = userId,
                ActivityType = activityType,
                ActivityTimestamp = DateTimeOffset.UtcNow,
                SessionID = sessionId,
                Metadata = metadata ?? string.Empty
            };

            await _channel.Writer.WriteAsync(activity);
            return activity;
        }

        public async Task<UserActivity> LogUserActivity(
            string username,
            string email,
            string activityType,
            string sessionId,
            string? metadata = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("email is required", nameof(email));

            var normalizedEmail = email.Trim().ToLowerInvariant();

            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

            if (user == null)
            {
                user = new User
                {
                    Username = username,
                    Email = normalizedEmail
                };

                _db.Users.Add(user);

                try
                {
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    // Possible race condition: another request inserted the same email concurrently.
                    user = await _db.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

                    if (user == null)
                    {
                        // Re-throw the original DbUpdateException context (can't access it here; so throw a new one)
                        throw;
                    }
                }
            }

            return await LogUserActivity(user.ID, activityType, sessionId, metadata);
        }

        // ---------------- REPORTS ----------------

        public async Task<List<(long UserId, string Username, int Count)>> GetMostActiveUsers(int topN = 10)
        {
            var q = await _db.UserActivities
                .AsNoTracking()
                .GroupBy(a => a.UserID)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(topN)
                .ToListAsync();

            var userIds = q.Select(x => x.UserId).ToList();

            var users = await _db.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.ID))
                .ToDictionaryAsync(u => u.ID, u => u.Username);

            return q.Select(x => (x.UserId, users.TryGetValue(x.UserId, out var name) ? name : "Unknown", x.Count)).ToList();
        }

        public async Task<double> GetAverageActivitiesPerSession()
        {
            var any = await _db.UserActivities.AnyAsync();
            if (!any) return 0d;

            var avg = await _db.UserActivities
                .AsNoTracking()
                .GroupBy(a => a.SessionID)
                .Select(g => (double)g.Count())
                .AverageAsync();

            return double.IsNaN(avg) ? 0d : avg;
        }

        public async Task<int> GetDailyActiveUsers(DateTimeOffset dayUtc)
        {
            var start = new DateTimeOffset(dayUtc.Date, TimeSpan.Zero);
            var end = start.AddDays(1);

            return await _db.UserActivities
                .AsNoTracking()
                .Where(a => a.ActivityTimestamp >= start && a.ActivityTimestamp < end)
                .Select(a => a.UserID)
                .Distinct()
                .CountAsync();
        }

        public async Task<int> GetWeeklyActiveUsers(DateTimeOffset weekStartUtc)
        {
            var start = new DateTimeOffset(weekStartUtc.Date, TimeSpan.Zero);
            var end = start.AddDays(7);

            return await _db.UserActivities
                .AsNoTracking()
                .Where(a => a.ActivityTimestamp >= start && a.ActivityTimestamp < end)
                .Select(a => a.UserID)
                .Distinct()
                .CountAsync();
        }

        // Return nullable TimeSpan so callers can handle "not found"
        public async Task<TimeSpan?> GetSessionDuration(string sessionId)
        {
            var session = await _db.UserActivities
                .AsNoTracking()
                .Where(a => a.SessionID == sessionId)
                .GroupBy(a => a.SessionID)
                .Select(g => new { Min = g.Min(x => x.ActivityTimestamp), Max = g.Max(x => x.ActivityTimestamp) })
                .FirstOrDefaultAsync();

            if (session == null) return null;
            return session.Max - session.Min;
        }

        public async Task<List<(string SessionId, TimeSpan Duration)>> GetRecentSessionDurations(int take = 100)
        {
            var q = await _db.UserActivities
                .AsNoTracking()
                .GroupBy(a => a.SessionID)
                .Select(g => new { Session = g.Key, Min = g.Min(x => x.ActivityTimestamp), Max = g.Max(x => x.ActivityTimestamp) })
                .OrderByDescending(x => x.Max)
                .Take(take)
                .ToListAsync();

            return q.Select(x => (x.Session ?? string.Empty, x.Max - x.Min)).ToList();
        }

        // ---------------- BACKGROUND WRITER ----------------

        private async Task ProcessQueueAsync(CancellationToken token)
        {
            var buffer = new List<UserActivity>(_batchSize);

            try
            {
                while (await _channel.Reader.WaitToReadAsync(token))
                {
                    while (buffer.Count < _batchSize && _channel.Reader.TryRead(out var item))
                        buffer.Add(item);

                    if (buffer.Count > 0)
                    {
                        try
                        {
                            await _db.UserActivities.AddRangeAsync(buffer, token);
                            await _db.SaveChangesAsync(token);
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"Failed to write batch: {ex.Message}");
                        }
                        finally
                        {
                            buffer.Clear();
                        }
                    }
                    else
                    {
                        await Task.Delay(_flushInterval, token);
                    }
                }
            }
            catch (OperationCanceledException) { /* graceful shutdown */ }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Background writer fatal: {ex}");
                throw;
            }
        }

        // Graceful shutdown
        public async ValueTask DisposeAsync()
        {
            _channel.Writer.Complete();
            _cts.Cancel();

            try
            {
                await _backgroundWriter;
            }
            catch { /* ignore */ }

            _cts.Dispose();
        }
    }
}