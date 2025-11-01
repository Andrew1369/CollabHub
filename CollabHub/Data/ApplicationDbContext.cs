using CollabHub.Models;
using Microsoft.EntityFrameworkCore;

namespace CollabHub.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }


        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<Venue> Venues => Set<Venue>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Asset> Assets => Set<Asset>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Venue>()
            .HasOne(v => v.Organization)
            .WithMany(o => o.Venues)
            .HasForeignKey(v => v.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Event>()
            .HasOne(e => e.Venue)
            .WithMany(v => v.Events)
            .HasForeignKey(e => e.VenueId)
            .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Asset>()
            .HasOne(a => a.Event)
            .WithMany(e => e.Assets)
            .HasForeignKey(a => a.EventId)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
