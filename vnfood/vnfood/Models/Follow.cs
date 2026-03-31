namespace vnfood.Models
{
    public class Follow
    {
        public int Id { get; set; }

        // Người bấm Follow
        public string FollowerId { get; set; } = null!;
        public virtual ApplicationUser Follower { get; set; } = null!;

        // Người được Follow
        public string FolloweeId { get; set; } = null!;
        public virtual ApplicationUser Followee { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
