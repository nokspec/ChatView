using ChatView.Hubs;

namespace ChatView.Models.ChatView
{
    public class Room
    {
        public string RoomCode { get; set; }
        public string OwnerId { get; set; }

        public List<Client> Clients { get; set; }
    } 
}
