using Microsoft.AspNetCore.Identity;

namespace ChatView.Models.ChatView
{
    public class Client
    {
        public string ConnectionId { get; set; }
        public string Username { get; set; }
        public Roles Role { get; set; }
        public bool Muted { get; set; } = false;
    }
}
