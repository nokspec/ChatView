using ChatView.Models.ChatView;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace ChatView.Hubs
{
    public class ChatViewHub : Hub
    {
        // Store the current state of the video (playing or paused) and the current time
        private static bool _isPlaying;
        private static double _currentTime;

        public List<string> Rooms { get; set; }

        public void SetVideo(string url)
        {
            Clients.All.SendAsync("SetVideo", url);
        }

        // Handle the play event from a client
        public void Play()
        {
            // Update the state and broadcast it to all clients in the room
            _isPlaying = true;
            Clients.Group(Context.GetHttpContext().Request.Query["roomId"]).SendAsync("UpdatePlayState", _isPlaying);
        }

        // Handle the pause event from a client
        public void Pause()
        {
            // Update the state and broadcast it to all clients in the room
            _isPlaying = false;
            Clients.Group(Context.GetHttpContext().Request.Query["roomId"]).SendAsync("UpdatePlayState", _isPlaying);
        }

        // Handle the timeupdate event from a client
        public void TimeUpdate(double time)
        {
            // Update the current time and broadcast it to all clients in the room
            if (time != _currentTime)
            {
                _currentTime = time;
                Clients.Group(Context.GetHttpContext().Request.Query["roomId"]).SendAsync("UpdateTime", _currentTime);
            }
        }

        // Handle the seek event from a client
        public void Seek(double time)
        {
            // Update the current time and broadcast it to all clients in the room
            if (time != _currentTime)
            {
                _currentTime = time;
                Clients.Group(Context.GetHttpContext().Request.Query["roomId"]).SendAsync("UpdateTime", _currentTime);
            }
        }

        public void UpdateRooms(List<string> rooms)
        {
            //Rooms = new(); //onnodig vgm
            Rooms = rooms;
        }

        // Handle a client joining the room or channel
        public override async Task OnConnectedAsync()
        {
            string roomId = Context.GetHttpContext().Request.Query["roomId"];

            // Check if the room exists
            if (!Rooms.TryGetValue(roomId, out Room room))
            {
                //TOOD: add error handling for when a room doesnt exist
                return;
            }
            else
            {
                // If the room exists, add the new user to the list of users in the room
                room.Users.Add(Context.ConnectionId);
            }

            // Add the user to the SignalR group for the room
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            // Send the current state and time to the new client
            await Clients.Caller.SendAsync("UpdatePlayState", _isPlaying);
            await Clients.Caller.SendAsync("UpdateTime", _currentTime);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Remove the user from the room
            if (Context.Items.TryGetValue("room", out var roomId) &&
                Rooms.TryGetValue((string)roomId, out var room))
            {
                room.Users.Remove(Context.ConnectionId);

                // Notify other users in the room that the user has left
                await Clients.Group((string)roomId).SendAsync("UserLeftRoom", Context.ConnectionId);

                // If there are no more users in the room, remove the room
                if (room.Users.Count == 0)
                {
                    Rooms.TryRemove((string)roomId, out _);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }


        //chat
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", Context.User.Identity.Name, message);
        }
    }
}
