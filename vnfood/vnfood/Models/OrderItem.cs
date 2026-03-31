using System;

namespace vnfood.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int? PostId { get; set; }
        public int? ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        // Navigation
        public virtual Order Order { get; set; } = null!;
        public virtual Post? Post { get; set; }
        public virtual Product? Product { get; set; }
    }
}
