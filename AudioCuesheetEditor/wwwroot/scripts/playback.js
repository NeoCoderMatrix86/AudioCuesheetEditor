window.playAudio = (audioElement, objectUrl) => {
    audioElement.src = objectUrl;
    audioElement.play();
};

window.seekAudio = (audioElement, seconds) => {
    audioElement.currentTime = seconds;
};

window.pauseAudio = (audioElement) => {
    audioElement.pause();
};

window.stopAudio = (audioElement) => {
    audioElement.pause();
    audioElement.currentTime = 0;
}

window.getAudioCurrentTime = (audioElement) => {
    return audioElement.currentTime;
};