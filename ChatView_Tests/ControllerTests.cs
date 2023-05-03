using ChatView.Hubs;
using ChatView_API.Controllers;
using ChatView_API.DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ChatView_Tests
{
    public class ControllerTests
    {
        private DbContextOptions<ChatViewDbContext> _dbContextOptions;


        public ControllerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<ChatViewDbContext>()
                .UseInMemoryDatabase("TestDB_" + DateTime.Now.ToFileTimeUtc())
                .Options;
        }


        //Here we test if the API returns a 400 statuscode if an URL gets sent that is not a Youtube URL.
        [Fact]
        public async void VideoControllerShouldNotReturnOk()
        {
            ChatViewController apicontroller = new(new ChatViewDbContext(this._dbContextOptions));

            ChatView_API.Models.ChatView.NewVideo newVideo = new()
            {
                Url = "www.incorrecturl.com"
            };

            ObjectResult actual = (ObjectResult)await apicontroller.DownloadNewVideo(newVideo);

            Assert.Equal(new StatusCodeResult(400).StatusCode, actual.StatusCode);
        }

        //Here we test if the API returns a 200 statuscode if the URL is valid.
        [Fact]
        public async void VideoControllerShouldReturnOk()
        {
            ChatViewController apicontroller = new(new ChatViewDbContext(this._dbContextOptions));

            ChatView_API.Models.ChatView.NewVideo newVideo = new()
            {
                Url = "https://www.youtube.com/watch?v=PwyZteKCZt0&ab_channel=AnomalyClips"
            };

            ObjectResult actual = (ObjectResult)await apicontroller.DownloadNewVideo(newVideo);

            Assert.Equal(new StatusCodeResult(200).StatusCode, actual.StatusCode);
        }

    }
}
