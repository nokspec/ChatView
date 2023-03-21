var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

function createRoom() {
    connection.invoke("CreateRoom").catch(function (err) {
        console.error(err);
    });
}

connection.on("RoomCreated", function (roomId, url) {
    console.log(`Room created with id ${roomId}. URL: ${url}`);

    // Update the URL to include the room ID
    var newUrl = url + "?roomId=" + roomId;

    // Navigate to the new page
    window.location.href = newUrl;
});

connection.start().then(function () {
    console.log("Connection started.");
}).catch(function (err) {
    console.error(err.toString());
});