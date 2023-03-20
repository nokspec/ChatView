$(document).ready(function () {
	$('#videoForm').submit(function () {
		$('.urlbox').addClass('loading'); // add 'loading' class to the parent div
		$('.spinner').show(); // show the spinner
	});
});

document.getElementById('videoplayer').addEventListener('loadedmetadata', function () {
	document.querySelector('.urlbox').classList.remove('loading');
});

document.getElementById('videoplayer').addEventListener('loadedmetadata', function () {
	document.querySelector('.urlbox').classList.remove('loading');
	document.getElementById('videoForm').reset();
});

$(document).ready(function () {
	// Handle the form submission
	$('#videoForm').submit(function (event) {
		event.preventDefault(); // Prevent the default form submission
		var url = $('#URL').val(); // Get the URL from the input field
		$.ajax({
			url: '/ChatView/DownloadVideo',
			type: 'POST',
			data: { url: url },
			success: function (data) {
				$('#videoSource').attr('src', data); // Update the video source with the new URL
				connection.invoke('SetVideo', data); //Set the URL for all clients
				$('#videoplayer').get(0).load(); // Reload the video player
			},
			error: function (xhr, status, error) {
				console.log(error); // Log any errors to the console
			}
		});
	});
});

//video playback
const videoPlayer = document.getElementById('videoplayer');
const currentTimeSpan = document.getElementById('current-time');

const connection = new signalR.HubConnectionBuilder()
	.withUrl("/videohub")
	.configureLogging(signalR.LogLevel.Information)
	.build();

// Start the connection to the hub
connection.start().catch(err => console.error(err.toString()));

connection.on('SetVideo', function (url) {
	console.log("URL set");
	console.log(url);
	$('#videoSource').attr('src', url); // Update the video source with the new URL
	$('#videoplayer').get(0).load(); // Reload the video player
})

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
	}
	else if (!videoPlayer.paused && !isPlaying) {
		console.log("pause")
		videoPlayer.pause();
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