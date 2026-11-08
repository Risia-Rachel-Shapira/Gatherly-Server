using Gatherly.Core.Enums;
using Gatherly.Core.Models;
using Gatherly.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Gatherly.Data.Initialization
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
            AppDbContext context)
        {
            // Apply pending migrations
            await context.Database.MigrateAsync();

            // Prevent duplicate seed data
            if (await context.Users.AnyAsync())
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
    }
}