using ChatView.Models.ChatView;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace ChatView.Controllers
{
    public class ChatViewController : Controller
    {
        private readonly HttpClient _httpClient;
        private static readonly Regex _youTubeUrlRegex = new Regex(@"^(http(s)?:\/\/)?((w){3}.)?youtu(be|.be)?(\.com)?\/.+?$", RegexOptions.Compiled);

        public ChatViewController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [Authorize]
        public IActionResult ChatView()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DownloadVideo(string url)
        {
            Console.WriteLine("Nu API call MAKEN");
            if (string.IsNullOrEmpty(url) || !_youTubeUrlRegex.IsMatch(url))
            {
                return BadRequest("Invalid URL");
            }
            else
            {
                var apiUrl = "http://localhost/api/chatview/newvideo";
                Console.WriteLine(apiUrl);
                var payload = new NewVideo
                {
                    Url = url
                };

                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var videoUrl = await response.Content.ReadAsStringAsync();
                    var videoUrlTrimmed = string.Concat(videoUrl.Where(c => !Char.IsWhiteSpace(c)));

                    Console.WriteLine("Nu hoort yt video te komen");
                    return Json(videoUrlTrimmed);
                }
                return View("ChatView");
            }
        }
    }
}
