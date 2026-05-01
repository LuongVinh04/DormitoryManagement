namespace Dormitory.Models.Entities
{
    public class ChatMessage : BaseEntity
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;

        public Users? Sender { get; set; }
        public Users? Receiver { get; set; }
    }
}
