using ChatView.Hubs;

namespace ChatView.Models.ChatView
{
    public class Room
    {
        public string Id { get; set; }
        public List<string> Users { get; set; }
        public ChatViewHub ChatViewHub { get; set; }
    }

}
