using System;
using System.ComponentModel.DataAnnotations;

namespace CollabHub.Models
{
    public class Asset
    {
        public int Id { get; set; }


        [Required]
        public int EventId { get; set; }
        public Event? Event { get; set; }


        public string FilePath { get; set; } = string.Empty; // відносний шлях типу "/uploads/abc.png"


        [StringLength(200)]
        public string? OriginalFileName { get; set; }


        [StringLength(100)]
        public string? ContentType { get; set; }


        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
