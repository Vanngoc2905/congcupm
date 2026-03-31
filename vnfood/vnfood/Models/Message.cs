namespace vnfood.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public string SenderId { get; set; } = null!;
        public virtual ApplicationUser Sender { get; set; } = null!;

        public string ReceiverId { get; set; } = null!;
        public virtual ApplicationUser Receiver { get; set; } = null!;
    }
}
