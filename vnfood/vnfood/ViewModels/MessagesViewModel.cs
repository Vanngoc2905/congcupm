using vnfood.Models;

namespace vnfood.ViewModels
{
    public class MessagesViewModel
    {
        public List<ApplicationUser> Contacts { get; set; } = new List<ApplicationUser>();
        public ApplicationUser? ActiveContact { get; set; }
        public List<Message> Conversation { get; set; } = new List<Message>();
    }
}
