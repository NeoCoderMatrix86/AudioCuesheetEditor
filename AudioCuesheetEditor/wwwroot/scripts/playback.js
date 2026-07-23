window.audioInterop = {
    playAudio: (audioElement, objectUrl) => {
        audioElement.src = objectUrl;
        audioElement.play();
    },

    seekAudio: (audioElement, seconds) => {
        audioElement.currentTime = seconds;
    },

    pauseAudio: (audioElement) => {
        audioElement.pause();
    },

    stopAudio: (audioElement) => {
        audioElement.pause();
        audioElement.currentTime = 0;
    },

    getAudioCurrentTime: (audioElement) => {
        return audioElement.currentTime;
    }
};