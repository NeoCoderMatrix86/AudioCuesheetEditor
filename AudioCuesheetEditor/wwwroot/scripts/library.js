var GLOBAL = {};
var audioFileObjectURL = null;
var handleAudioRecordingData = true;
var startTime;
GLOBAL.Index = null;
GLOBAL.AudioPlayer = null;
GLOBAL.ViewModeRecord = null;
GLOBAL.SetIndexReference = function (dotNetReference) {
    if (GLOBAL.Index === null) {
        GLOBAL.Index = dotNetReference;
    }
};
GLOBAL.SetAudioPlayerReference = function (dotNetReference) {
    if (GLOBAL.AudioPlayer === null) {
        GLOBAL.AudioPlayer = dotNetReference;
    }
};

GLOBAL.SetViewModeRecordReference = function (dotNetReference) {
    if (GLOBAL.ViewModeRecord === null) {
        GLOBAL.ViewModeRecord = dotNetReference;
    }
};

function getObjectURL(domId) {
    if (audioFileObjectURL != null) {
        URL.revokeObjectURL(audioFileObjectURL);
    }
    var element = document.getElementById(domId);
    var file = null;
    for (var i = 0, f; f = element.files[i]; i++) {
        if (f.type.startsWith("audio/")) {
            file = f;
        }
    }
    if (file != null) {
        audioFileObjectURL = URL.createObjectURL(file);
    }
    return audioFileObjectURL;
}

function triggerClick(domId) {
    var element = document.getElementById(domId);
    element.click();
}

function focusElement(domId) {
    var element = document.getElementById(domId);
    element.focus();
}

function resetLocalStorage() {
    localStorage.clear();
}

window.ApplicationOptions = {
    get: () => localStorage['ApplicationOptions'],
    set: (value) => localStorage['ApplicationOptions'] = value
}

window.addEventListener("keydown", function (e) {
    switch (e.key) {
        case "MediaPlayPause":
            GLOBAL.AudioPlayer.invokeMethodAsync("MediaPlayPausePressed");
            break;
        case "MediaNextTrack":
            GLOBAL.AudioPlayer.invokeMethodAsync("MediaNextTrackPressed");
            break;
        case "MediaPrevTrack":
            GLOBAL.AudioPlayer.invokeMethodAsync("MediaPreviousTrackPressed");
            break;
    }
});


//TODO: Remove this handling, when ASP.CORE 6 has global exception handling (https://github.com/dotnet/aspnetcore/issues/13452)
function removeBrowserHistoryEntry() {
    window.history.replaceState({}, 'ErrorReport', '/');
}

function reportError(error) {
    if (GLOBAL.Index !== null) {
        GLOBAL.Index.invokeMethodAsync("NotifyError", error);
    }
}

var exLog = console.error;
console.error = function (msg) {
    exLog.apply(console, arguments);
    reportError(msg);
}

window.addEventListener("unhandledrejection", function (promiseRejectionEvent) {
    reportError(promiseRejectionEvent.reason.message);
});

function dragLeave(e, domElement) {
    e.preventDefault();
    e.stopPropagation();
    domElement.classList.remove('is-dragover');
}

function dragOver(e, domElement) {
    e.preventDefault();
    e.stopPropagation();
    domElement.classList.add('is-dragover');
}

function dropFiles(e, domElement, domID) {
    e.preventDefault();
    e.stopPropagation();
    domElement.classList.remove('is-dragover');
    var dropedFiles = e.dataTransfer.files;
    var element = document.getElementById(domID);
    element.files = dropedFiles;
    var event = new Event('change');
    element.dispatchEvent(event);
}

function setupAudioRecording() {
    navigator.mediaDevices.getUserMedia({ audio: true }).then(stream => { handleAudioRecording(stream) }).catch(function (err) { handleAudioRecordingData = false; });
}

function handleAudioRecording(stream) {
    rec = new MediaRecorder(stream);
    rec.ondataavailable = e => {
        audioChunks.push(e.data);
    }
    rec.onstop = () => {
        var duration = Date.now() - startTime;
        let buggyBlob = new Blob(audioChunks, { 'type': 'audio/ogg; codecs=opus' });
        ysFixWebmDuration(buggyBlob, duration, function (fixedBlob) {
            var url = URL.createObjectURL(fixedBlob);
            if (GLOBAL.ViewModeRecord !== null) {
                GLOBAL.ViewModeRecord.invokeMethodAsync("AudioRecordingFinished", url);
            } 
        });
    }
}

function startAudioRecording() {
    if (handleAudioRecordingData == true) {
        startTime = Date.now();
        audioChunks = [];
        rec.start();
    }
}

function stopAudioRecording() {
    if (handleAudioRecordingData == true) {
        rec.stop();
    }
}