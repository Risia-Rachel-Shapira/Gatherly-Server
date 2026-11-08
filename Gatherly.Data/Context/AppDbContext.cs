using Gatherly.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Gatherly.Data.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventMember> EventMembers { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<ItemClaim> ItemClaims { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Contribution> Contributions { get; set; }

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            RefreshItemConcurrencyTokens();

            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            RefreshItemConcurrencyTokens();

            return base.SaveChanges();
        }

        private void RefreshItemConcurrencyTokens()
        {
            foreach (var entry in ChangeTracker.Entries<Item>())
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.Version = Guid.NewGuid();
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Event - Organizer
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organizer)
                .WithMany()
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);

            // EventMember
            modelBuilder.Entity<EventMember>()
                .HasOne(em => em.Event)
                .WithMany(e => e.Members)
                .HasForeignKey(em => em.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventMember>()
                .HasOne(em => em.User)
                .WithMany(u => u.EventMemberships)
                .HasForeignKey(em => em.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventMember>()
                .HasIndex(em => new { em.EventId, em.UserId })
                .IsUnique();

            // Invitation
            modelBuilder.Entity<Invitation>()
                .HasOne(i => i.Event)
                .WithMany(e => e.Invitations)
                .HasForeignKey(i => i.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invitation>()
                .HasIndex(i => i.Token)
                .IsUnique();

            // Item
            modelBuilder.Entity<Item>()
                .Property(i => i.Version)
                .IsConcurrencyToken();

            modelBuilder.Entity<Item>()
                .HasOne(i => i.Event)
                .WithMany(e => e.Items)
                .HasForeignKey(i => i.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.AddedByUser)
                .WithMany()
                .HasForeignKey(i => i.AddedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ItemClaim
            modelBuilder.Entity<ItemClaim>()
                .HasOne(c => c.Item)
                .WithMany()
                .HasForeignKey(c => c.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ItemClaim>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Expense
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.Event)
                .WithMany()
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Expense>()
                .HasOne(e => e.Item)
                .WithMany()
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Expense>()
                .HasOne(e => e.PaidByUser)
                .WithMany()
                .HasForeignKey(e => e.PaidByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Contribution
            modelBuilder.Entity<Contribution>()
                .HasOne(c => c.Event)
                .WithMany()
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contribution>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Active item claims
            modelBuilder.Entity<ItemClaim>()
                .HasIndex(c => new { c.ItemId, c.IsActive });

            // Decimal precision
            modelBuilder.Entity<Event>()
                .Property(e => e.Budget)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Item>()
                .Property(i => i.EstimatedCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Item>()
                .Property(i => i.ActualCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Expense>()
                .Property(e => e.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Contribution>()
                .Property(c => c.Amount)
                .HasPrecision(18, 2);
        }
    }
}