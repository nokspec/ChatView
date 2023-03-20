using System;
using System.Web;
using Microsoft.AspNetCore.SignalR;

namespace ChatView.Hubs
{
	public class ChatHub : Hub
	{
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
