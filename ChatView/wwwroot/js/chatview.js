const videoPlayer = document.getElementById('videoplayer');
const currentTimeSpan = document.getElementById('current-time');

var videoUrlElem = document.getElementById("videoUrl");
if (videoUrlElem != null) {
	var videoUrl = document.getElementById("videoUrl").getAttribute('src');
	console.log(videoUrl);
}
else {
	console.log("No video loaded");
}

// Create a new SignalR connection
const connection = new signalR.HubConnectionBuilder()
	.withUrl("/videohub")
	.configureLogging(signalR.LogLevel.Information)
	.build();

// Start the connection to the hub
connection.start().catch(err => console.error(err.toString()));

// Handle the play event from the video player
videoPlayer.addEventListener("play", function () {
	// Send the play event to the hub
	console.log("play");
	connection.invoke('Play');
	connection.invoke("TimeUpdate", videoPlayer.currentTime);

});

// Handle the pause event from the video player
videoPlayer.addEventListener("pause", function () {
	// Send the pause event to the hub
	if (videoPlayer.play) {
		console.log("pause")
		connection.invoke('Pause');
		connection.invoke("TimeUpdate", videoPlayer.currentTime);

	}
});

// Handle the UpdatePlayState event from the hub
connection.on('UpdatePlayState', function (isPlaying) {
	console.log("UpdatePlayState");
	if (videoPlayer.paused && isPlaying) {
		console.log("play")
		videoPlayer.play();
		//connection.invoke("TimeUpdate", videoPlayer.currentTime);
	}
	else if (!videoPlayer.paused && !isPlaying) {
		console.log("pause")
		videoPlayer.pause();
		//connection.invoke("TimeUpdate", videoPlayer.currentTime);
	}
});

// Handle the UpdateTime event from the hub
connection.on('UpdateTime', function (currentTime) {
	console.log("sync");
	// Set the current time of the video
	videoPlayer.currentTime = currentTime;
});

// Handle the seeked event from the video player
videoPlayer.addEventListener('seeked', function () {
	console.log("seeked");
	// Send the seek event to the hub
	connection.invoke('Seek', videoPlayer.currentTime);
	connection.invoke("TimeUpdate", videoPlayer.currentTime);
});