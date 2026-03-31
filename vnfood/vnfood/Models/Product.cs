using System;
using System.Collections.Generic;

namespace vnfood.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; } = 0;
        
        public string? ImageUrl { get; set; } // Main image
        public string? ImageUrl2 { get; set; }
        public string? ImageUrl3 { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        public int CategoryId { get; set; }
        public string UserId { get; set; } = null!;

        // Navigation
        public virtual Category Category { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
        
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
