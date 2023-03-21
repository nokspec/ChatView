using ChatView.Models.ChatView;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Collections.Concurrent;

namespace ChatView.Controllers
{
    public class RoomController : Controller
    {
        private static readonly ConcurrentDictionary<string, Room> Rooms =
        new(StringComparer.OrdinalIgnoreCase);

        public IActionResult Room(string roomId)
        {
            var room = Rooms.FirstOrDefault(r => r.Key == roomId).Value;

            if (room == null)
            {
                return NotFound();
            }

            ViewData["RoomId"] = roomId;

            return View("ChatView");
        }

    }
}