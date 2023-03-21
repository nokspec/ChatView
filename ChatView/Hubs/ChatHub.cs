using System;
using System.Collections.Concurrent;
using System.Security.Policy;
using System.Web;
using ChatView.Models.ChatView;
using Microsoft.AspNetCore.SignalR;

namespace ChatView.Hubs
{
	public class ChatHub : Hub
	{
        private static readonly ConcurrentDictionary<string, Room> Rooms =
        new(StringComparer.OrdinalIgnoreCase);

        //public async Task ChatHub_CreateRoom()
        //{
        //    var roomId = Guid.NewGuid().ToString();
        //    var roomName = Guid.NewGuid().ToString();

        //    var room = new Room
        //    {
        //        Id = roomId,
        //        Name = roomName,
        //        Users = new List<string> { Context.ConnectionId },
        //        ChatHub = this, // Add reference to this hub
        //        VideoHub = null // Set reference to video player hub to null initially
        //    };

        //    Rooms.TryAdd(roomId, room);

        //    await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

        //    var url = $"{Context.GetHttpContext().Request.Scheme}://{Context.GetHttpContext().Request.Host}/Chat/Room/{roomId}";

        //    await Clients.Caller.SendAsync("RoomCreated", roomId, url);
        //}

        //public async Task CreateRoom()
        //{
        //    await ChatHub_CreateRoom();
        //}

        //private async Task ChatHub_CreateRoom()
        //{
        //    var roomId = Guid.NewGuid().ToString();
        //    var roomName = Guid.NewGuid().ToString();

        //    var room = new Room
        //    {
        //        Id = roomId,
        //        Name = roomName,
        //        Users = new List<string> { Context.ConnectionId },
        //        ChatHub = this, // Add reference to this hub
        //    };

        //    Rooms.TryAdd(roomId, room);

        //    await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

        //    var url = $"{Context.GetHttpContext().Request.Scheme}://{Context.GetHttpContext().Request.Host}/Chat/Room/{roomId}";

        //    await Clients.Caller.SendAsync("RoomCreated", roomId, url);
        //}

        //public async Task ChatHub_JoinRoom(string roomId)
        //{
        //    await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        //    await Clients.Caller.SendAsync("ChatHub_JoinedRoom", roomId);
        //}


    }
}
