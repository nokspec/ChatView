using ChatView.Hubs;
using ChatView_API.Controllers;
using ChatView_API.DAL;
using ChatView_API.Models.ChatView;
using ChatView_API.Models.Db_Models;
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


        /// <summary>
        /// Here we test if the API returns a 400 statuscode if an URL gets sent that is not a Youtube URL.
        /// </summary>
        [Fact]
        public async void VideoControllerShouldNotReturnOk()
        {
            ChatViewController apicontroller = new(new ChatViewDbContext(this._dbContextOptions));

            NewVideo newVideo = new()
            {
                Url = "www.incorrecturl.com"
            };

            ObjectResult actual = (ObjectResult)await apicontroller.DownloadNewVideo(newVideo);

            Assert.Equal(new StatusCodeResult(400).StatusCode, actual.StatusCode);
        }

        /// <summary>
        /// Here we test if the API returns a 200 statuscode if the URL is valid.
        /// </summary>
        [Fact]
        public async void VideoControllerShouldReturnOk()
        {
            ChatViewController apicontroller = new(new ChatViewDbContext(this._dbContextOptions));

            NewVideo newVideo = new()
            {
                Url = "https://www.youtube.com/watch?v=PwyZteKCZt0&ab_channel=AnomalyClips"
            };

            ObjectResult actual = (ObjectResult)await apicontroller.DownloadNewVideo(newVideo);

            Assert.Equal(new StatusCodeResult(200).StatusCode, actual.StatusCode);
        }

        /// <summary>
        /// Test if a video is already in the database
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task VideoControllerShouldReturnExistingVideo()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ChatViewDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            // Create a new in-memory database
            using (var context = new ChatViewDbContext(options))
            {
                var existingVideo = new DbVideo()
                {
                    Id = 1,
                    YoutubeUrl = "https://www.youtube.com/watch?v=PwyZteKCZt0&ab_channel=AnomalyClips",
                    Mp4Bytes = new byte[] { 1, 2, 3, 4, 5 }
                };

                context.Videos.Add(existingVideo);
                await context.SaveChangesAsync();
            }

            // Use the actual DbContext with the in-memory database
            using (var context = new ChatViewDbContext(options))
            {
                var controller = new ChatViewController(context);

                var newVideo = new NewVideo()
                {
                    Url = "https://www.youtube.com/watch?v=PwyZteKCZt0&ab_channel=AnomalyClips"
                };

                // Act
                var actionResult = await controller.DownloadNewVideo(newVideo);
                var okObjectResult = Assert.IsType<OkObjectResult>(actionResult);
                var actualValue = okObjectResult.Value.ToString();

                // Assert
                var expectedBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 });
                Assert.Equal($"data:video/mp4;base64,{expectedBase64}", actualValue);
            }
        }
    }
}
