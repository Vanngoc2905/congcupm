using System;
using System.ComponentModel.DataAnnotations;

namespace vnfood.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;

        public string? SenderId { get; set; }
        public virtual ApplicationUser? Sender { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        public string? LinkUri { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
