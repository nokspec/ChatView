using Microsoft.AspNetCore.SignalR;

namespace ChatView.Hubs
{
    public class VideoHub : Hub
    {
        // Store the current state of the video (playing or paused) and the current time
        private static bool isPlaying = false;
        private static double currentTime = 0;

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
            if (time != currentTime) //sync every 30 seconds TODO: optimize solution
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
    }
}
