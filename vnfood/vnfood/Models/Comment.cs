using System;

namespace vnfood.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public string UserId { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;

        public int PostId { get; set; }
        public virtual Post Post { get; set; } = null!;
    }
}
