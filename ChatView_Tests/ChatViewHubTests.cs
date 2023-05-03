using ChatView.Hubs;
using ChatView.Models.ChatView;
using Microsoft.AspNetCore.SignalR;
using Moq;
using NUnit.Framework;

namespace ChatView_Tests
{
    //if these tests fail, make sure to comment all the
    //awaits in the JoinRoom method.

    public class ChatViewHubTests
    {

        private ChatViewHub hub;
        private Mock<HubCallerContext> contextMock;
        private Mock<IHubCallerClients> clientsMock;

        public ChatViewHubTests()
        {
            hub = new ChatViewHub();
            contextMock = new Mock<HubCallerContext>();
            clientsMock = new Mock<IHubCallerClients>();
            hub.Context = contextMock.Object;
            hub.Clients = clientsMock.Object;   
        }

        [Fact]
        public async Task JoinRoom_NewRoom_RoomAdded()
        {
            // Arrange
            string roomCode = "123";
            string username = "testuser";
            contextMock.SetupGet(c => c.ConnectionId).Returns("testconnectionid");
            contextMock.SetupGet(c => c.User.Identity.Name).Returns(username);

            // Act
            await hub.JoinRoom(roomCode);

            // Assert
            NUnit.Framework.Assert.That(hub.Rooms.Count, Is.EqualTo(1));
            NUnit.Framework.Assert.That(hub.Rooms[0].RoomCode, Is.EqualTo(roomCode));
            NUnit.Framework.Assert.That(hub.Rooms[0].OwnerId, Is.EqualTo("testconnectionid"));
            NUnit.Framework.Assert.That(hub.Rooms[0].Clients.Count, Is.EqualTo(1));
            NUnit.Framework.Assert.That(hub.Rooms[0].Clients[0].Username, Is.EqualTo(username));
            NUnit.Framework.Assert.That(hub.Rooms[0].Clients[0].Role, Is.EqualTo(ChatView.Models.ChatView.Roles.Admin));

        }

        [Fact]
        public async Task JoinRoom_ExistingRoom_ClientAdded()
        {
            // Arrange
            string roomCode = "123";
            string username = "testuser";
            var room = new Room
            {
                RoomCode = roomCode,
                OwnerId = "ownerid",
                Clients = new List<Client>()
            };
            room.Clients.Add(new Client
            {
                ConnectionId = "existingclientconnectionid",
                Username = "existingclientusername",
                Role = (ChatView.Models.ChatView.Roles)ChatView.Hubs.Roles.Admin
            });
            hub.Rooms.Add(room);
            contextMock.SetupGet(c => c.ConnectionId).Returns("testconnectionid");
            contextMock.SetupGet(c => c.User.Identity.Name).Returns(username);

            // Act
            await hub.JoinRoom(roomCode);

            // Assert
            NUnit.Framework.Assert.That(hub.Rooms.Count, Is.EqualTo(1));
            NUnit.Framework.Assert.That(hub.Rooms[0].RoomCode, Is.EqualTo(roomCode));
            NUnit.Framework.Assert.That(hub.Rooms[0].OwnerId, Is.EqualTo("ownerid"));
            NUnit.Framework.Assert.That(hub.Rooms[0].Clients.Count, Is.EqualTo(2));
            NUnit.Framework.Assert.That(hub.Rooms[0].Clients[1].ConnectionId, Is.EqualTo("testconnectionid"));
            NUnit.Framework.Assert.That(hub.Rooms[0].Clients[1].Username, Is.EqualTo(username));
            NUnit.Framework.Assert.That(hub.Rooms[0].Clients[1].Role, Is.EqualTo(ChatView.Models.ChatView.Roles.Viewer));
        }
    }
}
