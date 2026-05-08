namespace WebParfum.Models;

public class Message
{
    public int Id { get; set; }

    public int SenderId { get; set; }
    public User Sender { get; set; } = null!;

    public int RecipientId { get; set; }
    public User Recipient { get; set; } = null!;

    public string Body { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
