var chatviewConnection = new signalR.HubConnectionBuilder()
	.withUrl("/chatviewhub")
	.configureLogging(signalR.LogLevel.Information)
	.build(); //Create a connection

chatviewConnection.start().catch(err => console.error(err.toString()));

window.onload = function () {
	document.getElementById("createButton").addEventListener("click", (event) => {
		chatviewConnection.invoke("CreateRoomAsync").catch(function (err) {
			return console.error(err.toString());
		});
		event.preventDefault();
	});
}