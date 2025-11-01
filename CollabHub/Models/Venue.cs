using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollabHub.Models
{
    public class Venue
    {
        public int Id { get; set; }


        [Required]
        public int OrganizationId { get; set; }
        public Organization? Organization { get; set; }


        [Required, StringLength(120)]
        public string Name { get; set; } = string.Empty;


        [StringLength(200)]
        public string? Address { get; set; }


        public double? Latitude { get; set; }
        public double? Longitude { get; set; }


        public List<Event> Events { get; set; } = new();
    }
}
