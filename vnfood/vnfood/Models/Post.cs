using System;
using System.Collections.Generic;

namespace vnfood.Models
{
    public class Post
    {
        public int Id { get; set; }
        
        // --- RECIPE FIELDS ---
        public string? Title { get; set; }
        public string? Content { get; set; } // Description
        public string? PrepTime { get; set; } // Time
        public string? Servings { get; set; } // People count
        public string? Ingredients { get; set; } // List
        public string? Instructions { get; set; } // Steps
        
        // --- IMAGES ---
        public string? ImageUrl { get; set; } // Main image / Avatar
        public string? ImageUrl2 { get; set; }
        public string? ImageUrl3 { get; set; }
        public string? ImageUrl4 { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key
        public string UserId { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;

        // Navigation
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}
