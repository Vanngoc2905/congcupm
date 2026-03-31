using System;

namespace vnfood.Models
{
    public class Like
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public string UserId { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;

        public int PostId { get; set; }
        public virtual Post Post { get; set; } = null!;
    }
}
