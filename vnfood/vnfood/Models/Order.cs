using System;
using System.Collections.Generic;

namespace vnfood.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        
        // Shipping Info
        public string FullName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        
        public string PaymentMethod { get; set; } = "COD";

        // Status: Chờ xác nhận, Đang chuẩn bị, Đang giao, Hoàn thành, Đã hủy
        public string Status { get; set; } = "Chờ xác nhận"; 

        // Navigation
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
