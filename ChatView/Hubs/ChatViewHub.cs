using ChatView.Models.ChatView;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Policy;

namespace ChatView.Hubs
{
    public class ChatViewHub : Hub
    {
        // Store the current state of the video (playing or paused) and the current time
        private static bool isPlaying = false;
        private static double currentTime = 0;

        private static readonly ConcurrentDictionary<string, Room> Rooms =
        new(StringComparer.OrdinalIgnoreCase);

        public async Task CreateRoom()
        {
            await ChatViewHub_CreateRoom();
        }

        private async Task ChatViewHub_CreateRoom()
        {
            var roomId = Guid.NewGuid().ToString();
            var roomName = Guid.NewGuid().ToString();

            var room = new Room
            {
                Id = roomId,
                Name = roomName,
                Users = new List<string> { Context.ConnectionId },
                ChatViewHub = this, // Add reference to this hub
            };

            Rooms.TryAdd(roomId, room);

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            var url = $"{Context.GetHttpContext().Request.Scheme}://{Context.GetHttpContext().Request.Host}/Chat/Room/{roomId}";

            await Clients.Caller.SendAsync("RoomCreated", roomId, url);
        }

        public async Task VideoHub_JoinRoom(string roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Caller.SendAsync("VideoHub_JoinedRoom", roomId);
        }

        public void SetVideo(string url)
        {
            Clients.All.SendAsync("SetVideo", url);
        }

        // Handle the play event from a client
        public void Play()
        {
            // Update the state and broadcast it to all clients
            isPlaying = true;
            Clients.All.SendAsync("UpdatePlayState", isPlaying);
        }

        // Handle the pause event from a client
        public void Pause()
        {
            // Update the state and broadcast it to all clients
            isPlaying = false;
            Clients.All.SendAsync("UpdatePlayState", isPlaying);
        }

        // Handle the timeupdate event from a client
        public void TimeUpdate(double time)
        {
            // Update the current time and broadcast it to all clients
            if (time != currentTime)
            {
                currentTime = time;
                Clients.All.SendAsync("UpdateTime", currentTime);
            }
        }

        // Handle a client joining the room or channel
        public override async Task OnConnectedAsync()
        {
            // Send the current state and time to the new client
            await Clients.Caller.SendAsync("UpdatePlayState", isPlaying);
            await Clients.Caller.SendAsync("UpdateTime", currentTime);

            await base.OnConnectedAsync();
        }

        // Handle the seek event from a client
        public void Seek(double time)
        {
            // Update the current time and broadcast it to all clients
            if (time != currentTime)
            {
                currentTime = time;
                Clients.All.SendAsync("UpdateTime", currentTime);
            }
        }

        //chat
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

    }
}
