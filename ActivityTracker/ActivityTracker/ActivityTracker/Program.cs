using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ActivityTracker.Models;
using ActivityTracker.Services;

namespace ActivityTracker
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var connectionString = @"Server=localhost\MSSQLSERVER01;Database=UserActivityDB;Trusted_Connection=True;TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<ActivityContext>()
                .UseSqlServer(connectionString)
                .Options;

            // create the context first so it's visible to EnsureCreated and the menu code
            await using var context = new ActivityContext(options);

            try
            {
                // Use migrations in real apps. EnsureCreated is quick for demo.
                context.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database creation check failed: " + ex.Message);
                Console.WriteLine("Make sure connection string & SQL Server are reachable. Press Enter to continue...");
                Console.ReadLine();
            }

            await using var activityService = new ActivityService(context);

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Activity Tracker - Menu");
                Console.WriteLine("1) Log a test activity (creates user if needed)");
                Console.WriteLine("2) Most active users (top 10)");
                Console.WriteLine("3) Average activities per session");
                Console.WriteLine("4) DAU (enter yyyy-mm-dd)");
                Console.WriteLine("5) WAU (enter yyyy-mm-dd for week start)");
                Console.WriteLine("6) Session duration (enter SessionID)");
                Console.WriteLine("7) Recent session durations (top 10)");
                Console.WriteLine("8) Seed test data (bulk; requires Microsoft.Data.SqlClient)");
                Console.WriteLine("0) Exit");
                Console.Write("Choice: ");

                if (!int.TryParse(Console.ReadLine(), out var choice))
                {
                    Console.WriteLine("Invalid choice. Press Enter to continue...");
                    Console.ReadLine();
                    continue;
                }

                try
                {
                    switch (choice)
                    {
                        case 0:
                            return;
                        case 1:
                            await LogTestActivity(context, activityService);
                            break;
                        case 2:
                            await MostActiveUsers(activityService);
                            break;
                        case 3:
                            await AvgActivitiesPerSession(activityService);
                            break;
                        case 4:
                            await DAU(activityService);
                            break;
                        case 5:
                            await WAU(activityService);
                            break;
                        case 6:
                            await SessionDuration(activityService);
                            break;
                        case 7:
                            await RecentSessionDurations(activityService);
                            break;
                        case 8:
                            Console.Write("Num users to seed (e.g. 100): ");
                            if (!int.TryParse(Console.ReadLine() ?? "100", out var numUsers)) numUsers = 100;
                            Console.Write("Activities per user (e.g. 10): ");
                            if (!int.TryParse(Console.ReadLine() ?? "10", out var activitiesPerUser)) activitiesPerUser = 10;
                            await Seeder.SeedBulk(context, numUsers, activitiesPerUser);
                            Console.WriteLine("Seed complete. Press Enter to continue...");
                            Console.ReadLine();
                            break;
                        default:
                            Console.WriteLine("Unknown option. Press Enter to continue...");
                            Console.ReadLine();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine("Press Enter to continue...");
                    Console.ReadLine();
                }
            }
        }

        static async Task LogTestActivity(ActivityContext db, ActivityService svc)
        {
            Console.Write("Username: ");
            var username = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(username))
            {
                Console.WriteLine("Username required. Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.Write("Email (leave blank to use username@example.local): ");
            var email = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(email)) email = $"{username}@example.local";

            Console.Write("Activity type (default PageView): ");
            var type = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(type)) type = "PageView";

            var session = Guid.NewGuid().ToString();

            var activity = await svc.LogUserActivity(username, email, type, session, "{}");

            Console.WriteLine($"Enqueued activity for user {username}. Press Enter to continue...");
            Console.ReadLine();
        }

        static async Task MostActiveUsers(ActivityService svc)
        {
            var top = await svc.GetMostActiveUsers(10);
            foreach (var (userId, username, count) in top)
            {
                Console.WriteLine($"{username} -> {count}");
            }
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        static async Task AvgActivitiesPerSession(ActivityService svc)
        {
            var avg = await svc.GetAverageActivitiesPerSession();
            Console.WriteLine($"Avg actions per session: {avg:F2}");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        static async Task DAU(ActivityService svc)
        {
            Console.Write("Date (yyyy-mm-dd): ");
            var text = Console.ReadLine();
            if (!DateTimeOffset.TryParse(text, out var date))
            {
                Console.WriteLine("Invalid date. Press Enter...");
                Console.ReadLine();
                return;
            }

            var dau = await svc.GetDailyActiveUsers(date);
            Console.WriteLine($"DAU for {date:yyyy-MM-dd} = {dau}");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        static async Task WAU(ActivityService svc)
        {
            Console.Write("Week start date (yyyy-mm-dd): ");
            var text = Console.ReadLine();
            if (!DateTimeOffset.TryParse(text, out var start))
            {
                Console.WriteLine("Invalid date. Press Enter...");
                Console.ReadLine();
                return;
            }

            var wau = await svc.GetWeeklyActiveUsers(start);
            Console.WriteLine($"WAU starting {start:yyyy-MM-dd} = {wau}");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        static async Task SessionDuration(ActivityService svc)
        {
            Console.Write("SessionID: ");
            var sessionId = Console.ReadLine();
            if (string.IsNullOrEmpty(sessionId))
            {
                Console.WriteLine("No session id provided. Press Enter...");
                Console.ReadLine();
                return;
            }

            var duration = await svc.GetSessionDuration(sessionId);
            if (duration == null)
            {
                Console.WriteLine("Session not found.");
            }
            else
            {
                Console.WriteLine($"{sessionId} -> {duration}");
            }

            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        static async Task RecentSessionDurations(ActivityService svc)
        {
            var list = await svc.GetRecentSessionDurations(10);
            foreach (var (sid, duration) in list)
            {
                Console.WriteLine($"{sid} -> {duration}");
            }
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
    }
}