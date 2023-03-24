using ChatView.Models.ChatView;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Policy;

namespace ChatView.Hubs
{
    public enum Roles
    {
        Admin,
        Moderator,
        Viewer
    }

    public class ChatViewHub : Hub
    {
        // Store the current state of the video (playing or paused) and the current time
        private static bool _isPlaying;
        private static double _currentTime;

        private static readonly List<Room> Rooms = new();
        private static readonly ConcurrentDictionary<Client, Room> ClientRoomDictionary = new();
        private static readonly ConcurrentDictionary<string, string> Users = new();

        /// <summary>
        /// If a user creates a room, create new entry, otherwise add the user to the corresponding room
        /// </summary>
        /// <param name="roomcode"></param>
        /// <returns></returns>
        public async Task JoinRoom(string roomcode)
        {
            Client client = new();
            client.ConnectionId = Context.ConnectionId;
            client.Username = Context.User.Identity.Name;

            // check if room exists, if not, make a new room. Otherwise join it.
            Room room = Rooms.FirstOrDefault(x => x.RoomCode == roomcode);
            if (room is null)
            {
                room = new Room
                {
                    RoomCode = roomcode,
                    OwnerId = client.ConnectionId,
                    Clients = new List<Client> { client } // Add the client to the list of clients in the room.
                };
                Rooms.Add(room);
                client.Role = Models.ChatView.Roles.Admin;
                await Clients.Clients(client.ConnectionId).SendAsync("AddVideoPlayer");
            }
            else
            {
                client.Role = Models.ChatView.Roles.Viewer;
                room.Clients.Add(client); // Add the client to the list of clients in the existing room.
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, roomcode);

            ClientRoomDictionary.TryAdd(client, room);

            var user = Context.User.Identity.Name;
            Users.TryAdd(user, room.RoomCode);

            await GetUserList();
        }

        public async Task HandleSelectOption(string option, string username)
        {
            var ClientKVP = ClientRoomDictionary.FirstOrDefault(x => x.Key.ConnectionId == Context.ConnectionId);
            var room = await GetRoom();

            //Check if the user is authorized to perform the action
            if (ClientKVP.Key.Role == Models.ChatView.Roles.Admin || ClientKVP.Key.Role == Models.ChatView.Roles.Mod)
            {
                var userToUpdate = ClientRoomDictionary.FirstOrDefault(x => x.Key.Username == username);
                if (option.Equals("promote") && (userToUpdate.Key.Role != Models.ChatView.Roles.Mod || userToUpdate.Key.Role != Models.ChatView.Roles.Admin))
                {
                    userToUpdate.Key.Role = Models.ChatView.Roles.Mod;
                    //Update videoplayer controls for the promoted user
                    await Clients.Clients(userToUpdate.Key.ConnectionId).SendAsync("AddVideoPlayer");
                    await Clients.Group(room.RoomCode).SendAsync("ReceiveMessage", userToUpdate.Key.Username, " has been promoted");
                }

                else if (option.Equals("demote") && userToUpdate.Key.Role == Models.ChatView.Roles.Mod)
                {
                    userToUpdate.Key.Role = Models.ChatView.Roles.Viewer;
                    await Clients.Clients(userToUpdate.Key.ConnectionId).SendAsync("RemoveVideoPlayer");
                    await Clients.Group(room.RoomCode).SendAsync("ReceiveMessage", userToUpdate.Key.Username, " has been demoted");
                }

                else if (option.Equals("mute") && userToUpdate.Key.Role == Models.ChatView.Roles.Viewer)
                {
                    userToUpdate.Key.Muted = true;
                    await Clients.Clients(userToUpdate.Key.ConnectionId).SendAsync("UserMuted");
                    await Clients.Group(room.RoomCode).SendAsync("ReceiveMessage", userToUpdate.Key.Username, " has been muted");
                }
                else if (option.Equals("unmute") && userToUpdate.Key.Role == Models.ChatView.Roles.Viewer)
                {
                    userToUpdate.Key.Muted = false;
                    await Clients.Clients(userToUpdate.Key.ConnectionId).SendAsync("UserUnmuted");
                    await Clients.Group(room.RoomCode).SendAsync("ReceiveMessage", userToUpdate.Key.Username, " has been unmuted");
                }
            }
        }

        /// <summary>
        /// Gets the room based on client
        /// </summary>
        /// <returns></returns>
        public async Task<Room> GetRoom()
        {
            var ClientKVP = ClientRoomDictionary.FirstOrDefault(x => x.Key.ConnectionId == Context.ConnectionId);
            return ClientKVP.Value;
        }

        /// <summary>
        /// Gets the client data of the current user
        /// </summary>
        /// <returns></returns>
        public async Task<Client> GetClient()
        {
            var ClientKVP = ClientRoomDictionary.FirstOrDefault(x => x.Key.ConnectionId == Context.ConnectionId);
            return ClientKVP.Key;
        }

        public async Task<List<string>> GetUserList()
        {
            var room = await GetRoom();
            List<string> result = new();
            foreach (var user in Users)
            {
                if (user.Value == room.RoomCode && !result.Contains(user.Value))
                    result.Add(user.Key);
            }
            await Clients.Group(room.RoomCode).SendAsync("createUserList", result);
            return result;
        }

        /// <summary>
        /// Get the Id of the current room
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetRoomId()
        {
            var room = await GetRoom();
            return room.RoomCode;
        }

        /// <summary>
        /// Set the video URL for all clients
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task SetVideo(string url)
        {
            var room = await GetRoom();
            var user = Context.User.Identity.Name;

            if (room != null)
            {
                // Set a new video for all users.
                await Clients.Group(room.RoomCode).SendAsync("SetVideo", url);

                //Notify all users that a new video has been set
                await Clients.Group(room.RoomCode).SendAsync("ReceiveMessage", user, "added a new video");
            }
        }

        public async Task GetUser()
        {
            var user = await GetClient();
            await Clients.Clients(user.ConnectionId).SendAsync("GetUser", user.Username);
        }

        public async Task VideoLoading()
        {
            var room = await GetRoom();
            await Clients.Group(room.RoomCode).SendAsync("UrlLoading");
        }

        /// <summary>
        /// Handle the play event from a client
        /// </summary>
        /// <returns></returns>
        public async Task Play()
        {
            var user = await GetClient();
            if (user.Role != Models.ChatView.Roles.Viewer)
            {
                var room = await GetRoom();
                if (room != null)
                {
                    // Update the state and broadcast it to all clients
                    _isPlaying = true;
                    await Clients.Group(room.RoomCode).SendAsync("UpdatePlayState", _isPlaying);
                }
            }
        }

        /// <summary>
        /// Handle the pause event from a client
        /// </summary>
        /// <returns></returns>
        public async Task Pause()
        {
            var user = await GetClient();
            if (user.Role != Models.ChatView.Roles.Viewer)
            {
                var room = await GetRoom();

                if (room != null)
                { // Update the state and broadcast it to all clients
                    _isPlaying = false;
                    await Clients.Group(room.RoomCode).SendAsync("UpdatePlayState", _isPlaying);
                }
            }
        }

        /// <summary>
        /// Handle the timeupdate event from a client
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>        
        public async Task TimeUpdate(double time)
        {
            var user = await GetClient();
            if (user.Role != Models.ChatView.Roles.Viewer)
            {
                var room = await GetRoom();

                if (room != null)
                {
                    // Update the current time and broadcast it to all clients
                    if (time != _currentTime)
                    {
                        _currentTime = time;
                        await Clients.Group(room.RoomCode).SendAsync("UpdateTime", _currentTime);
                    }
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
            var user = await GetClient();
            if (user.Role != Models.ChatView.Roles.Viewer)
            {
                var room = await GetRoom();
                if (room != null)
                {
                    // Update the current time and broadcast it to all clients
                    if (time != _currentTime)
                    {
                        _currentTime = time;
                        await Clients.Group(room.RoomCode).SendAsync("UpdateTime", _currentTime);
                    }
                }
            }
        }

        /// <summary>
        /// When all the users have disconnected from the room, delete the room.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception ex)
        {
            var room = await GetRoom();

            if (room != null)
            {

                //TODO: when no one is in a room anymore, delete the room.
                //if (room.Clients.Count == 0)
                //{
                //}

                //delete the user from the room
                await Groups.AddToGroupAsync(Context.ConnectionId, room.RoomCode);

                Client client = new();
                client.ConnectionId = Context.ConnectionId;

                Room roomOut = new Room();
                ClientRoomDictionary.TryRemove(client, out roomOut);

                string value;
                var user = Context.User.Identity.Name;
                Users.TryRemove(user, out value);

                var userList = GetUserList();

                //update the userlist
                await Clients.Group(room.RoomCode).SendAsync("createUserList");
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
            var user = ClientRoomDictionary.FirstOrDefault(x => x.Key.ConnectionId == Context.ConnectionId);

            if (!user.Key.Muted)
            {
                var room = await GetRoom();
                if (room != null)
                {
                    await Clients.Group(room.RoomCode).SendAsync("ReceiveMessage", user.Key.Username, message);
                }
            }
            await Clients.Clients(user.Key.ConnectionId).SendAsync("UserMuted");
        }
    }
}
