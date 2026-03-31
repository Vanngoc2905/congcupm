using vnfood.Models;

namespace vnfood.ViewModels
{
    public class SearchViewModel
    {
        public string Query { get; set; } = string.Empty;
        public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public List<Post> Posts { get; set; } = new List<Post>();
        public HashSet<int> LikedPostIds { get; set; } = new HashSet<int>();
    }
}
