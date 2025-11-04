using System.ComponentModel.DataAnnotations;

namespace CollabHub.Models
{
    public class TodoItem
    {
        public int Id { get; set; }

        [Required, MaxLength(500)]
        public string Text { get; set; } = string.Empty;

        public bool Done { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Для порядку в списку (drag-n-drop)
        public int Order { get; set; }
    }
}
