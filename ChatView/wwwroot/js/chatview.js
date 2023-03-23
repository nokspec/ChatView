class ChatView {
	constructor() {
		this.videoPlayer = document.getElementById('videoplayer');
		this.currentTimeSpan = document.getElementById('current-time');

		this.connection = new signalR.HubConnectionBuilder()
			.withUrl(`/chatviewhub`)
			.configureLogging(signalR.LogLevel.Information)
			.build();

		this.connection.start().then(function () {
			document.getElementById("sendButton").disabled = false;
		}).catch(err => console.error(err.toString()));
		this.bindEvents();
		this.bindSignalREvents();
		this.bindFormSubmit();
		this.resetForm();
		this.chatButton();
	}

	createRoom(roomcode) {
		this.connection.invoke("JoinRoom", roomcode);
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

		this.connection.on("ReceiveMessage", function (user, message) {
			var li = document.createElement("li");
			document.getElementById("messagesList").appendChild(li);
			li.textContent = `${user}: ${message}`;
		});
	}

	bindFormSubmit() {
		const videoForm = document.getElementById('videoForm');
		videoForm.addEventListener('submit', (event) => {
			event.preventDefault();
			const url = document.getElementById('URL').value;
			this.fetchVideo(url);
		});


		const createRoom = document.getElementById('createRoom');
		createRoom.addEventListener('click', (event) => {
			event.preventDefault();
			const roomcode = document.getElementById('roomcode').value;
			this.createRoom(roomcode);
			$('.lobby').hide();
			$('.room').show();
			this.getRoomId();
		})

		const joinRoom = document.getElementById('joinRoom');
		joinRoom.addEventListener('click', (event) => {
			event.preventDefault();
			const roomcode = document.getElementById('roomcode').value;
			this.createRoom(roomcode);
			$('.lobby').hide();
			$('.room').show();
			this.getRoomId();
		})
	}

	getRoomId() {
		this.connection.invoke("GetRoomId").then(function (roomId) {
			document.getElementById("roomId").textContent = "Room code: " + roomId || "Couldn't retrieve room code";
		});
	}

	//TODO: misschien later nog vervangen met queryselector omdat jquery voor extra overhead zorgt en we het maar 2x gebruiken
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
				self.connection.invoke('SetVideo', data);
				$('#videoplayer').get(0).load(); // Reload the video player
				console.log("video set");
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

	chatButton() {
		document.getElementById("sendButton").addEventListener("click", (event) => {
			var message = document.getElementById("messageInput").value;
			this.connection.invoke("SendMessage", message).catch(function (err) {
				return console.error(err.toString());
			});
			event.preventDefault();
		});
	}
}

const chatView = new ChatView();
