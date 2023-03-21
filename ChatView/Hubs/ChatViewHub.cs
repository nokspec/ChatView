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

		private static readonly ConcurrentDictionary<string, Room> Rooms =
		new(StringComparer.OrdinalIgnoreCase);

		public void SetVideo(string url)
		{
			Clients.All.SendAsync("SetVideo", url);
		}

		// Handle the play event from a client
		public void Play()
		{
			// Update the state and broadcast it to all clients
			_isPlaying = true;
			Clients.All.SendAsync("UpdatePlayState", _isPlaying);
		}

		// Handle the pause event from a client
		public void Pause()
		{
			// Update the state and broadcast it to all clients
			_isPlaying = false;
			Clients.All.SendAsync("UpdatePlayState", _isPlaying);
		}

		// Handle the timeupdate event from a client
		public void TimeUpdate(double time)
		{
			// Update the current time and broadcast it to all clients
			if (time != _currentTime)
			{
				_currentTime = time;
				Clients.All.SendAsync("UpdateTime", _currentTime);
			}
		}

		// Handle a client joining the room or channel
		public override async Task OnConnectedAsync()
		{
			// Send the current state and time to the new client
			await Clients.Caller.SendAsync("UpdatePlayState", _isPlaying);
			await Clients.Caller.SendAsync("UpdateTime", _currentTime);

			await base.OnConnectedAsync();
		}

		// Handle the seek event from a client
		public void Seek(double time)
		{
			// Update the current time and broadcast it to all clients
			if (time != _currentTime)
			{
				_currentTime = time;
				Clients.All.SendAsync("UpdateTime", _currentTime);
			}
		}

		//chat
		public async Task SendMessage(string user, string message)
		{
			await Clients.All.SendAsync("ReceiveMessage", user, message);
		}
	}
}
