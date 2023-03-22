using ChatView.Hubs;

namespace ChatView.Models.ChatView
{
    public class Room
    {
        public string RoomId { get; set; }
        public List<string> Users { get; } = new List<string>();
        public string OwnerId { get; set; }

        public Room()
        {
            Users = new List<string>();
        }

        public void AddUser(string userId)
        {
            if (!Users.Contains(userId))
            {
                Users.Add(userId);
            }
        }

        public void RemoveUser(string userId)
        {
            Users.Remove(userId);
        }
    }

}
