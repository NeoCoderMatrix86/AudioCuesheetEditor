window.audioInterop = {
    dotnetRef: null,
    audioElement: null,

    register: (dotnetObject, audioElement) => {
        window.audioInterop.dotnetRef = dotnetObject;
        window.audioInterop.audioElement = audioElement;

        audioElement.addEventListener('ended', () => {
            window.audioInterop.dotnetRef.invokeMethodAsync('OnPlaybackEnded');
        });
    },

    playAudio: (objectUrl) => {
        window.audioInterop.audioElement.src = objectUrl;
        window.audioInterop.audioElement.play();
    },

    seekAudio: (seconds) => {
        window.audioInterop.audioElement.currentTime = seconds;
    },

    pauseAudio: () => {
        window.audioInterop.audioElement.pause();
    },

    stopAudio: () => {
        window.audioInterop.audioElement.pause();
        window.audioInterop.audioElement.currentTime = 0;
    },

    getAudioCurrentTime: () => {
        return window.audioInterop.audioElement.currentTime;
    },
};