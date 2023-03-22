using ChatView.Hubs;
using ChatView.Models.ChatView;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http;
using System.Security.Claims;
using System.Text;

namespace ChatView.Controllers
{
    public class RoomController : Controller
    {
        private readonly HttpClient _httpClient;

        //TOOD: miss is dict beter? id van db als key en roomId als value
        public List<string> Rooms { get; set; }
        private readonly IHubContext<ChatViewHub> _chatHubContext;


        public RoomController(HttpClient httpClient, IHubContext<ChatViewHub> chatHubContext)
        {
            _httpClient = httpClient;
            _chatHubContext = chatHubContext;
            Rooms = new();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom()
        {
            var apiUrl = "http://localhost:5134/api/room/newroom";
            var payload = new Room
            {
                RoomId = Guid.NewGuid().ToString(),
                OwnerId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            };
            payload.Users.Add(payload.OwnerId);

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var roomId = await response.Content.ReadAsStringAsync();
                Rooms.Add(roomId); //add the new room to the list of rooms
                await _chatHubContext.Clients.All.SendAsync("UpdateRooms", Rooms);

                return RedirectToAction("ChatView", "ChatView", new { roomId = payload.RoomId });
            }
            return View("Index");
        }
    }
}
