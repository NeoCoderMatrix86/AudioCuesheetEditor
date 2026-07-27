window.audioInterop = {
    dotnetRef: null,
    audioElement: null,
    eventHandlers: {
        onPlay: null,
        onEnded: null,
        onPause: null
    },

    register: (dotnetObject) => {
        const audio = new Audio();
        window.audioInterop.dotnetRef = dotnetObject;
        window.audioInterop.audioElement = audio;

        window.audioInterop.eventHandlers.onPlay = () => {
            window.audioInterop.dotnetRef.invokeMethodAsync('OnPlaybackStarted');
        };

        window.audioInterop.eventHandlers.onEnded = () => {
            const currentObjectUrl = window.audioInterop.audioElement.src;
            window.audioInterop.dotnetRef.invokeMethodAsync('OnPlaybackEnded', currentObjectUrl);
        };

        window.audioInterop.eventHandlers.onPause = () => {
            window.audioInterop.dotnetRef.invokeMethodAsync('OnPlaybackPaused');
        };

        window.audioInterop.audioElement.addEventListener('play', window.audioInterop.eventHandlers.onPlay);
        window.audioInterop.audioElement.addEventListener('ended', window.audioInterop.eventHandlers.onEnded);
        window.audioInterop.audioElement.addEventListener('pause', window.audioInterop.eventHandlers.onPause);
    },

    unregister: () => {
        if (window.audioInterop.audioElement) {
            window.audioInterop.audioElement.removeEventListener('play', window.audioInterop.eventHandlers.onPlay);
            window.audioInterop.audioElement.removeEventListener('ended', window.audioInterop.eventHandlers.onEnded);
            window.audioInterop.audioElement.removeEventListener('pause', window.audioInterop.eventHandlers.onPause);
        }

        window.audioInterop.dotnetRef = null;
        window.audioInterop.audioElement = null;
        window.audioInterop.eventHandlers.onPlay = null;
        window.audioInterop.eventHandlers.onEnded = null;
        window.audioInterop.eventHandlers.onPause = null;
    },

    setAudioSource: (objectUrl) => {
        window.audioInterop.audioElement.src = objectUrl;
    },

    playAudio: () => {
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
    }
};