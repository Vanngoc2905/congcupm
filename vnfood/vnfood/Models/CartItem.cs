using System;

namespace vnfood.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public int? PostId { get; set; }
        public int? ProductId { get; set; }
        public int Quantity { get; set; }

        // Navigation
        public virtual Post? Post { get; set; }
        public virtual Product? Product { get; set; }
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
