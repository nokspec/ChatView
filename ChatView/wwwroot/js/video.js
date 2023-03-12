const video = document.getElementById('videoplayer');
const currentTimeSpan = document.getElementById('current-time');

video.addEventListener('timeupdate', () => {
    const currentTime = video.currentTime;
    currentTimeSpan.textContent = currentTime.toFixed(1);
});

video.addEventListener('play', () => {
    console.log('The video has started playing.');
});

video.addEventListener('pause', () => {
    console.log('The video has been paused.');
});

video.addEventListener('ended', () => {
    console.log('The video has ended.');
});

video.addEventListener('seeked', () => {
    console.log('The video has been seeked to a new position.');
});