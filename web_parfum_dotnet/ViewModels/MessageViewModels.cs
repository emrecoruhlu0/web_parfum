using WebParfum.Models;

namespace WebParfum.ViewModels;

public class ConversationSummary
{
    public User OtherUser { get; set; } = null!;
    public Message LastMessage { get; set; } = null!;
    public int UnreadCount { get; set; }
}

public class MessagesIndexViewModel
{
    public List<ConversationSummary> Conversations { get; set; } = [];
}

public class ConversationViewModel
{
    public User OtherUser { get; set; } = null!;
    public List<Message> Messages { get; set; } = [];
}
