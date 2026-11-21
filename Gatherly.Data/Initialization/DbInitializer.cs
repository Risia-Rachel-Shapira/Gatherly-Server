using Gatherly.Core.Enums;
using Gatherly.Core.Models;
using Gatherly.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gatherly.Data.Initialization
{
    public static class DbInitializer
    {
        public const string AdminEmail = "admin@gatherly.local";

        public static async Task InitializeAsync(
            AppDbContext context,
            string? adminPassword = null,
            ILogger? logger = null)
        {
            // Apply pending migrations
            await context.Database.MigrateAsync();

            // Runs before the "already seeded" check, so the admin is
            // also created on databases that already have users.
            await EnsureAdminAsync(context, adminPassword, logger);

            // Prevent duplicate seed data
            if (await context.Users.AnyAsync(u => u.Email != AdminEmail))
                return;

            var passwordHasher = new PasswordHasher<User>();

            var organizer = new User
            {
                FullName = "Demo Organizer",
                Email = "organizer@gatherly.local",
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            organizer.PasswordHash =
                passwordHasher.HashPassword(
                    organizer,
                    "Organizer123!");

            var participant = new User
            {
                FullName = "Demo Participant",
                Email = "participant@gatherly.local",
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            participant.PasswordHash =
                passwordHasher.HashPassword(
                    participant,
                    "Participant123!");

            context.Users.AddRange(organizer, participant);

            await context.SaveChangesAsync();

            var gatherlyEvent = new Event
            {
                Name = "Gatherly Demo Event",
                Description = "Demo event for testing Gatherly",
                Date = DateTime.UtcNow.AddDays(7),
                Location = "Tel Aviv",
                Budget = 1000,
                OrganizerId = organizer.Id,
                ManagementType = EventManagementType.Both
            };

            context.Events.Add(gatherlyEvent);

            await context.SaveChangesAsync();

            var membership = new EventMember
            {
                EventId = gatherlyEvent.Id,
                UserId = participant.Id,
                IsActive = true,
                JoinedAt = DateTime.UtcNow,
                Role = MemberRole.Participant.ToString()
            };

            context.EventMembers.Add(membership);

            var items = new List<Item>
            {
                new Item
                {
                    EventId = gatherlyEvent.Id,
                    Name = "Drinks",
                    Description = "Bottles of drinks",
                    Quantity = 5,
                    EstimatedCost = 50,
                    AddedByUserId = organizer.Id
                },
                new Item
                {
                    EventId = gatherlyEvent.Id,
                    Name = "Snacks",
                    Description = "Snacks for participants",
                    Quantity = 3,
                    EstimatedCost = 80,
                    AddedByUserId = organizer.Id
                }
            };

            context.Items.AddRange(items);

            await context.SaveChangesAsync();
        }

        private static async Task EnsureAdminAsync(
            AppDbContext context,
            string? adminPassword,
            ILogger? logger)
        {
            var existing = await context.Users
                .FirstOrDefaultAsync(u => u.Email == AdminEmail);

            if (existing != null)
            {
                // Never promote an existing account: anyone could have
                // registered this email through the public API.
                if (existing.Role != UserRole.Admin)
                    logger?.LogWarning(
                        "User {Email} exists but is not an Admin; it was not promoted.",
                        AdminEmail);
                return;
            }

            // The password comes from configuration (Seed:AdminPassword),
            // never from source code.
            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                logger?.LogWarning(
                    "Seed:AdminPassword is not set; the demo admin {Email} was not created.",
                    AdminEmail);
                return;
            }

            var admin = new User
            {
                FullName = "Demo Admin",
                Email = AdminEmail,
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };

            admin.PasswordHash = new PasswordHasher<User>()
                .HashPassword(admin, adminPassword);

            context.Users.Add(admin);
            await context.SaveChangesAsync();

            logger?.LogInformation("Demo admin {Email} was created.", AdminEmail);
        }
    }
}