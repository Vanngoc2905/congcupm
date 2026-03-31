using System;
using System.Collections.Generic;

namespace vnfood.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = "fa-tag"; // FontAwesome class
        
        // Navigation
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
