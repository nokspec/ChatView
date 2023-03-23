using ChatView.Models;
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

        private static readonly List<Room> Rooms = new();
        private static readonly ConcurrentDictionary<Client, Room> ClientRoomDictionary = new();

        /// <summary>
        /// If a user creates a room, create new entry, otherwise add the user to the corresponding room
        /// </summary>
        /// <param name="roomcode"></param>
        /// <returns></returns>
        public async Task JoinRoom(string roomcode)
        {
            Client client = new();
            client.ConnectionId = Context.ConnectionId;

            // check if room exists, if not, make a new room. otherwise join it
            Room room = Rooms.FirstOrDefault(x => x.RoomId == roomcode);
            if (room is null)
            {
                room = new Room
                {
                    RoomId = roomcode,
                    OwnerId = client.ConnectionId,
                    Clients = new List<Client> { client } // Add the client to the list of clients in the room.
                };
                Rooms.Add(room);
            }
            else
            {
                room.Clients.Add(client); // Add the client to the list of clients in the existing room.
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, roomcode);
            ClientRoomDictionary.TryAdd(client, room);
        }

        /// <summary>
        /// Gets the room based on client
        /// </summary>
        /// <returns></returns>
        public async Task<Room> GetRoom()
        {
            var ClientKVP = (ClientRoomDictionary.FirstOrDefault(x => x.Key.ConnectionId == Context.ConnectionId));
            return ClientKVP.Value;
        }

        public async Task<string> GetRoomId()
        {
            var room = await GetRoom();
            return room.RoomId;
        }

        /// <summary>
        /// Set the video URL for all clients
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task SetVideo(string url)
        {
            var room = await GetRoom();

            if (room != null)
            {
                // Set a new video for all users.
                await Clients.Group(room.RoomId).SendAsync("SetVideo", url);
            }
        }

        /// <summary>
        /// Handle the play event from a client
        /// </summary>
        /// <returns></returns>
        public async Task Play()
        {
            var room = await GetRoom();
            if (room != null)
            {
                // Update the state and broadcast it to all clients
                _isPlaying = true;
                await Clients.Group(room.RoomId).SendAsync("UpdatePlayState", _isPlaying);
            }
        }

        /// <summary>
        /// Handle the pause event from a client
        /// </summary>
        /// <returns></returns>
        public async Task Pause()
        {
            var room = await GetRoom();

            if (room != null)
            { // Update the state and broadcast it to all clients
                _isPlaying = false;
                await Clients.Group(room.RoomId).SendAsync("UpdatePlayState", _isPlaying);
            }
        }

        /// <summary>
        /// Handle the timeupdate event from a client
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>        
        public async Task TimeUpdate(double time)
        {
            var room = await GetRoom();

            if (room != null)
            {
                // Update the current time and broadcast it to all clients
                if (time != _currentTime)
                {
                    _currentTime = time;
                    await Clients.Group(room.RoomId).SendAsync("UpdateTime", _currentTime);
                }
            }
        }

        /// <summary>
        /// Handle the seek event from a client
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public async Task Seek(double time)
        {
            var room = await GetRoom();
            if (room != null)
            {
                // Update the current time and broadcast it to all clients
                if (time != _currentTime)
                {
                    _currentTime = time;
                    await Clients.Group(room.RoomId).SendAsync("UpdateTime", _currentTime);
                }
            }
        }

        /// <summary>
        /// Handle a client joining the room or channel
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            // Send the current state and time to the new client
            await Clients.Caller.SendAsync("UpdatePlayState", _isPlaying);
            await Clients.Caller.SendAsync("UpdateTime", _currentTime);

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// When all the users have disconnected from the room, delete the room.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception ex)
        {
            var room = await GetRoom();

            //TODO: when no one is in a room anymore, delete the room.
            if (room.Clients.Count == 0)
            {
            }

            await base.OnDisconnectedAsync(ex);
        }

        /// <summary>
        /// Client sends a message to the other users in the corresponding room
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessage(string message)
        {
            var user = Context.User.Identity.Name;
            var room = await GetRoom();
            if (room != null)
            {
                await Clients.Group(room.RoomId).SendAsync("ReceiveMessage", user, message);
            }
        }
    }
}
