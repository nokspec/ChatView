class ChatView {
	constructor() {
		this.videoPlayer = document.getElementById('videoplayer');
		this.currentTimeSpan = document.getElementById('current-time');

		this.connection = new signalR.HubConnectionBuilder()
			.withUrl("/chatviewhub")
			.configureLogging(signalR.LogLevel.Information)
			.build(); //Create a connection
		this.connection.start().catch(err => console.error(err.toString())); //Start the connection

		this.bindEvents();
		this.bindSignalREvents();
		this.bindFormSubmit();
		this.resetForm();
	}

	bindEvents() {
		this.videoPlayer.addEventListener("play", () => {
			console.log("play");
			this.connection.invoke('Play');
			this.connection.invoke("TimeUpdate", this.videoPlayer.currentTime);
		});

		this.videoPlayer.addEventListener("pause", () => {
			if (this.videoPlayer.play) {
				console.log("pause")
				this.connection.invoke('Pause');
				this.connection.invoke("TimeUpdate", this.videoPlayer.currentTime);
			}
		});

		this.videoPlayer.addEventListener('seeked', () => {
			console.log("seeked");
			this.connection.invoke('Seek', this.videoPlayer.currentTime);
			this.connection.invoke("TimeUpdate", this.videoPlayer.currentTime);
		});
	}

	updatePlayState(isPlaying) {
		console.log("UpdatePlayState");
		if (this.videoPlayer.paused && isPlaying) {
			console.log("play")
			this.videoPlayer.play();
		} else if (!this.videoPlayer.paused && !isPlaying) {
			console.log("pause")
			this.videoPlayer.pause();
		}
	}

	updateTime(currentTime) {
		console.log("sync");
		this.videoPlayer.currentTime = currentTime;
	}

	bindSignalREvents() {
		this.connection.on('SetVideo', (url) => {
			console.log("URL set");
			console.log(url);
			$('#videoSource').attr('src', url);
			$('#videoplayer').get(0).load();
		});

		this.connection.on('UpdatePlayState', (isPlaying) => {
			this.updatePlayState(isPlaying);
		});

		this.connection.on('UpdateTime', (currentTime) => {
			this.updateTime(currentTime);
		});
	}

	bindFormSubmit() {
		const form = document.getElementById('videoForm');
		form.addEventListener('submit', (event) => {
			event.preventDefault();
			const url = document.getElementById('URL').value;
			this.fetchVideo(url);
		});
	}

	//misschien later nog vervangen met queryselector omdat jquery voor extra overhead zorgt en we het maar 2x gebruiken
	fetchVideo(url) {
		$('.urlbox').addClass('loading');
		$('.spinner').show();
		var self = this; // Save a reference to 'this'
		var url = $('#URL').val(); // Get the URL from the input field
		$.ajax({
			url: '/ChatView/DownloadVideo',
			type: 'POST',
			data: { url: url },
			success: function (data) {
				$('#videoSource').attr('src', data); // Update the video source with the new URL
				self.connection.invoke('SetVideo', data); // Use the saved reference to invoke the 'connection' method
				$('#videoplayer').get(0).load(); // Reload the video player
			},
			error: function (xhr, status, error) {
				console.log(error); // Log any errors to the console
			}
		});
	}

	resetForm() {
		const videoPlayer = document.getElementById('videoplayer');
		videoPlayer.addEventListener('loadedmetadata', () => {
			document.querySelector('.urlbox').classList.remove('loading');
			document.getElementById('videoForm').reset();
		});
	}
}

const chatView = new ChatView();

