using System;

namespace vnfood.Models
{
    public class HiddenPost
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public int PostId { get; set; }

        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Post Post { get; set; } = null!;
    }
}
