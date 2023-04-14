using ChatView_API.DAL;
using ChatView_API.Models.ChatView;
using ChatView_API.Models.Db_Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace ChatView_API.Controllers
{
    [ApiController]
    [Route("/api/chatview/newvideo")]
    public class ChatViewController : ControllerBase
    {
        private readonly ChatViewDbContext _context;
        private static readonly Regex _youTubeUrlRegex = new Regex(@"^(http(s)?:\/\/)?((w){3}.)?youtu(be|.be)?(\.com)?\/.+?$", RegexOptions.Compiled);

        public ChatViewController(ChatViewDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> DownloadNewVideo([FromBody] NewVideo newVideo)
        {
            if (string.IsNullOrEmpty(newVideo?.Url) || !_youTubeUrlRegex.IsMatch(newVideo.Url))
            {
                return BadRequest("Invalid URL");
            }
            else
            {
                try
                {
                    // Check if video already exists in database
                    var existingVideo = _context.Videos.FirstOrDefault(v => v.YoutubeUrl == newVideo.Url);

                    if (existingVideo != null)
                    {
                        // Video already exists, return its URL
                        var base64 = Convert.ToBase64String(existingVideo.Mp4Bytes);
                        var existingVideoUrl = $"data:video/mp4;base64,{base64}";
                        return Ok(existingVideoUrl);
                    }
                    else
                    {
                        // Video does not exist in Db, download it
                        var youtube = new YoutubeClient();
                        var videoUrl = newVideo.Url;
                        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoUrl);

                        // get the highest quality stream that has both audio and video
                        var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();

                        var stream = await youtube.Videos.Streams.GetAsync(streamInfo);

                        // Download the stream to a byte array
                        byte[] mp4Bytes;
                        using (var ms = new MemoryStream())
                        {
                            await stream.CopyToAsync(ms);
                            mp4Bytes = ms.ToArray();
                        }

                        // Save the MP4 bytes to the database
                        var video = new DbVideo
                        {
                            YoutubeUrl = videoUrl,
                            Mp4Bytes = mp4Bytes
                        };
                        _context.Videos.Add(video);
                        await _context.SaveChangesAsync();

                        // Convert MP4 bytes to base64 string and return
                        var base64 = Convert.ToBase64String(mp4Bytes);
                        var newVideoUrl = $"data:video/mp4;base64,{base64}";
                        return Ok(newVideoUrl);
                    }
                }
                catch (Exception ex)
                {
                    // Add error handling here
                    return StatusCode(500, $"An error occurred while downloading the video: {ex.Message}");
                }
            }
        }
    }
}
