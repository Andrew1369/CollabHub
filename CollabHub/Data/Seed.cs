using System;
using System.Linq;
using CollabHub.Models;
using Microsoft.EntityFrameworkCore;

namespace CollabHub.Data
{
    public static class Seed
    {
        public static void EnsureSeeded(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();


            if (!db.Organizations.Any())
            {
                var org = new Organization { Name = "Demo Org", Description = "Sample" };
                db.Organizations.Add(org);
                db.SaveChanges();


                var v = new Venue { Name = "Main Hall", OrganizationId = org.Id, Address = "Central St 1" };
                db.Venues.Add(v);
                db.SaveChanges();


                db.Events.Add(new Event { VenueId = v.Id, Title = "Launch Meetup", StartsAt = DateTime.UtcNow.AddDays(2), EndsAt = DateTime.UtcNow.AddDays(2).AddHours(2), Capacity = 100, ImageUrl = "https://picsum.photos/seed/demo/800/400" });
                db.SaveChanges();
            }
        }
    }
}
