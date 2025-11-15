using NuGet.ContentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CollabHub.Models
{
    public class Event
    {
        public int Id { get; set; }


        [Required]
        public int VenueId { get; set; }
        public Venue? Venue { get; set; }


        [Required, StringLength(140)]
        public string Title { get; set; } = string.Empty;


        [StringLength(2000)]
        public string? Description { get; set; }


        [DataType(DataType.DateTime)]
        public DateTime StartsAt { get; set; } = DateTime.UtcNow.AddDays(1);


        [DataType(DataType.DateTime)]
        public DateTime EndsAt { get; set; } = DateTime.UtcNow.AddDays(1).AddHours(2);


        public int? Capacity { get; set; }


        
        [Url]
        public string? ImageUrl { get; set; }


        public List<Asset> Assets { get; set; } = new();
    }
}
