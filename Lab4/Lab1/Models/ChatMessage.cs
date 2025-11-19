using System;

namespace Lab1.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int? MovieId { get; set; }           // якщо повідомлення прив'язане до фільму
        public string? ReceiverUserId { get; set; } // якщо приватне — кому адресовано
        public string SenderUserId { get; set; } = string.Empty;
        public string SenderUserName { get; set; } = string.Empty;
        public string? Text { get; set; }
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public bool IsPrivate { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
