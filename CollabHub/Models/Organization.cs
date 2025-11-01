using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CollabHub.Models
{
    public class Organization
    {
        public int Id { get; set; }


        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;


        [StringLength(200)]
        public string? Description { get; set; }


        public List<Venue> Venues { get; set; } = new();
    }
}
