using AuthSystem.Areas.Identity.Data;

namespace AuthSystem.Models
{
    public class ChatRoom
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsGroupChat { get; set; }
        public string CreatorId { get; set; }
        public virtual ApplicationUser Creator { get; set; }
        public virtual ICollection<ChatMessage> Messages { get; set; }
        public virtual ICollection<ChatParticipant> Participants { get; set; }
    }

    public class ChatMessage
    {
        public int Id { get; set; }
        public string? Content { get; set; } 
        public DateTime Timestamp { get; set; }
        public string SenderId { get; set; }
        public virtual ApplicationUser Sender { get; set; }
        public string? ImageUrl { get; set; } 
        public int ChatRoomId { get; set; }
        public virtual ChatRoom ChatRoom { get; set; }
    }

    public class ChatParticipant
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public int ChatRoomId { get; set; }
        public virtual ChatRoom ChatRoom { get; set; }
    }
}
