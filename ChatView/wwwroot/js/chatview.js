export default class ChatView {
	constructor() {
		this.videoPlayer = document.getElementById('videoplayer');
		this.currentTimeSpan = document.getElementById('current-time');

		this.connection = new signalR.HubConnectionBuilder()
			.withUrl(`/chatviewhub`)
			.configureLogging(signalR.LogLevel.Information)
			.build();

		this.bindEvents();
		this.bindSignalREvents();
		this.bindFormSubmit();
		this.resetForm();
		this.chatButton();

		this.connection.start().then(function () {
			document.getElementById("sendButton").disabled = false;
		}).catch(err => console.error(err.toString()));
	}

	createRoom(roomcode) {
		this.connection.invoke("JoinRoom", roomcode);
	}

	createUserList() {
		var ul = document.getElementById('userlist');
		while (ul.firstChild) ul.removeChild(ul.firstChild);

		this.connection.invoke("GetUserList").then((result) => {
			//console.log(result);
			result.forEach((user) => {
				var li = document.createElement('li');
				li.textContent = user;
				ul.appendChild(li);
			});
		});
	}

	bindSignalREvents() {
		this.connection.on("createuserlist", (userList) => {
			let ul = document.getElementById('userlist');
			ul.classList.add('list');
			ul.innerHTML = '';
			userList.forEach((user) => {
				let li = document.createElement('li');
				li.textContent = user;

				let select = document.createElement('select');
				let username = user;

				let choose = document.createElement('option');
				choose.textContent = "";
				choose.value = "choose";

				let promote = document.createElement('option');
				promote.textContent = "Promote";
				promote.value = "promote";

				let demote = document.createElement('option');
				demote.textContent = "Demote";
				demote.value = "demote";

				let mute = document.createElement('option');
				mute.textContent = "Mute";
				mute.value = "mute";

				let unmute = document.createElement('option');
				unmute.textContent = "Unmute";
				unmute.value = "unmute";

				let kick = document.createElement('option');
				kick.textContent = "Kick";
				kick.value = "kick";

				select.appendChild(choose);
				select.appendChild(promote);
				select.appendChild(demote);
				select.appendChild(mute);
				select.appendChild(unmute);
				select.appendChild(kick);

				let initialOptionValue = select.value;

				select.addEventListener("change", () => {
					let selectedIndex = select.selectedIndex;
					let selectedOption = select.options[selectedIndex];
					//console.log(selectedOption.value);
					//console.log(username);

					this.HandleSelectedOption(selectedOption.value, username);

					setTimeout(function () {
						select.value = initialOptionValue;
					}, 500);
				});

				li.appendChild(select);
				ul.appendChild(li);
			});
		});

		this.connection.on('SetVideo', (url) => {
			$('#videoSource').attr('src', url);
			$('#videoplayer').get(0).load();
		});

		this.connection.on('UpdatePlayState', (isPlaying) => {
			this.updatePlayState(isPlaying);
		});

		this.connection.on('UpdateTime', (currentTime) => {
			this.updateTime(currentTime);
		});

		this.connection.on("ReceiveMessage", (user, message) => {
			let li = document.createElement("li");

			let ul = document.getElementById("messagesList")
			ul.appendChild(li);
			ul.classList.add('list');

			li.textContent = `${user}: ${message}`;
		});

		this.connection.on("AddVideoPlayer", () => {
			this.videoPlayer.setAttribute("controls", "controls")
		})

		this.connection.on("RemoveVideoPlayer", () => {
			this.videoPlayer.removeAttribute("controls");
		})

		this.connection.on("UrlLoading", () => {
			this.urlLoading();
		});

		this.connection.on("UserMuted", () => {
			alert("You're muted");
		});

		this.connection.on("UserUnmuted", () => {
			alert("You've been unmuted");
		});

		this.connection.on("Unauthorized", () => {
			alert("Unauthorized");
		});

		this.connection.on("KickUser", () => {
			window.location.reload();
		});
	}

	HandleSelectedOption(option, user) {
		this.connection.invoke("HandleSelectOption", option, user);
	}

	bindEvents() {
		this.videoPlayer.addEventListener("play", () => {
			//console.log("play");
			this.connection.invoke('Play');
			this.connection.invoke("TimeUpdate", this.videoPlayer.currentTime);
		});

		this.videoPlayer.addEventListener("pause", () => {
			if (this.videoPlayer.play) {
				//console.log("pause")
				this.connection.invoke('Pause');
				this.connection.invoke("TimeUpdate", this.videoPlayer.currentTime);
			}
		});

		this.videoPlayer.addEventListener('seeked', () => {
			//console.log("seeked");
			this.connection.invoke('Seek', this.videoPlayer.currentTime);
			this.connection.invoke("TimeUpdate", this.videoPlayer.currentTime);
		});
	}

	updatePlayState(isPlaying) {
		//console.log("UpdatePlayState");
		if (this.videoPlayer.paused && isPlaying) {
			//console.log("play")
			this.videoPlayer.play();
		} else if (!this.videoPlayer.paused && !isPlaying) {
			//console.log("pause")
			this.videoPlayer.pause();
		}
	}

	updateTime(currentTime) {
		//console.log("sync");
		this.videoPlayer.currentTime = currentTime;
	}

	bindFormSubmit() {
		const videoForm = document.getElementById('videoForm');
		videoForm.addEventListener('submit', (event) => {
			event.preventDefault();
			const url = document.getElementById('URL').value;
			const regex = /^(http(s)?:\/\/)?((w){3}.)?youtu(be|.be)?(\.com)?\/.+$/;
			if (regex.test(url)) {
				this.fetchVideo(url);
				this.connection.invoke("VideoLoading");
			}
			else {
				alert("Please enter a valid URL");
			}
		});

		const createRoom = document.getElementById('createRoom');
		createRoom.addEventListener('click', (event) => {
			event.preventDefault();
			const roomcode = document.getElementById('roomcode').value;
			if (roomcode != '') {
				this.createRoom(roomcode);
				$('.lobby').hide();
				$('.room').show();
				this.getRoomId();
			}
			else {
				alert("Please enter a room code");
			}
		});

		const joinRoom = document.getElementById('joinRoom');
		joinRoom.addEventListener('click', (event) => {
			event.preventDefault();
			const roomcode = document.getElementById('roomcode').value;
			if (roomcode != '') {
				this.createRoom(roomcode);
				$('.lobby').hide();
				$('.room').show();
				this.getRoomId();
			}
			else {
				alert("Please enter a room code");
			}
		});
	}

	getRoomId() {
		this.connection.invoke("GetRoomId").then(function (roomId) {
			document.getElementById("roomId").textContent = "Room code: " + roomId || "Couldn't retrieve room code";
		});
	}

	urlLoading() {
		$('.urlbox').addClass('loading');
		$('.spinner').show();
	}

	fetchVideo(url) {
		this.urlLoading();
		const self = this;
		const response = await fetch('/ChatView/DownloadVideo', {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json'
			},
			body: JSON.stringify({ url })
		});
		if (!response.ok) {
			throw new Error('Failed to download video');
		}
		const data = await response.json();
		$('#videoSource').attr('src', data);
		self.connection.invoke('SetVideo', data);
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
			if (message != '') {
				this.connection.invoke("SendMessage", message).catch(function (err) {
					return console.error(err.toString());
				});
				document.getElementById("messageInput").value = '';
				event.preventDefault();
			}
			else {
				alert("Please enter a message");
			}
		});
	}
}

export { ChatView };
