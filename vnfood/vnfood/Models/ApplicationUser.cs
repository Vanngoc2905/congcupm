using Microsoft.AspNetCore.Identity;

namespace vnfood.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        
        // Navigation properties
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

        // Follow relationships
        public virtual ICollection<Follow> Followers { get; set; } = new List<Follow>();   // người follow mình
        public virtual ICollection<Follow> Following { get; set; } = new List<Follow>();   // mình follow người khác

        // Messages
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();

        // Notifications
        public virtual ICollection<Notification> NotificationsUnread { get; set; } = new List<Notification>();
        public virtual ICollection<Notification> NotificationsSent { get; set; } = new List<Notification>();
    }
}
