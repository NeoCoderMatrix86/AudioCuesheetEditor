var GLOBAL = {};
var fileObjectURL = null;
var startTime;
var mediaStream = null;
window.addEventListener('beforeunload', beforeunload);
GLOBAL.ViewModeRecord = null;

GLOBAL.SetViewModeRecordReference = function (dotNetReference) {
    if (GLOBAL.ViewModeRecord === null) {
        GLOBAL.ViewModeRecord = dotNetReference;
    }
};

function getObjectURLFromMudFileUpload(stackId) {
    if (fileObjectURL != null) {
        URL.revokeObjectURL(fileObjectURL);
    }
    var stackElement = document.getElementById(stackId);
    var inputElement = stackElement.querySelector('input[type="file"]');
    var file = null;
    for (var i = 0, f; f = inputElement.files[i]; i++) {
        if (f.type.startsWith("audio/")) {
            file = f;
        }
    }
    if (file != null) {
        fileObjectURL = URL.createObjectURL(file);
    }
    return fileObjectURL;
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

window.AppSettings = {
    get: (key) => localStorage[key],
    set: (key, value) => localStorage[key] = value
};

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
    navigator.mediaDevices.getUserMedia({ audio: { channelCount: 2, sampleRate: 480000 } }).then(stream => { handleAudioRecording(stream) }).catch(function () { });
}

function closeAudioRecording() {
    if (mediaStream !== null) {
        mediaStream.getTracks().forEach(function (track) {
            track.stop();
        });
    }
}

function handleAudioRecording(stream) {
    mediaStream = stream;
    const options = {
        audioBitsPerSecond: 320000,
        audioBitrateMode: "constant"
    }
    rec = new MediaRecorder(stream, options);
    rec.ondataavailable = e => {
        audioChunks.push(e.data);
    }
    rec.onstart = () => {
        startTime = Date.now();
    }
    rec.onstop = () => {
        var duration = Date.now() - startTime;
        closeAudioRecording();
        let buggyBlob = new Blob(audioChunks);
        ysFixWebmDuration(buggyBlob, duration, function (fixedBlob) {
            var url = URL.createObjectURL(fixedBlob);
            if (GLOBAL.ViewModeRecord !== null) {
                GLOBAL.ViewModeRecord.invokeMethodAsync("AudioRecordingFinished", url);
            } 
        });
        //ReSetup audio recording
        setupAudioRecording();
    }
}

function startAudioRecording() {
    if (mediaStream !== null) {
        audioChunks = [];
        rec.start();
    }
}

function stopAudioRecording() {
    if ((mediaStream !== null) && (rec.state !== 'inactive') && (rec.state !== 'stopped')) {
        rec.stop();
    }
}

function beforeunload(e) {
    e.preventDefault();
    e.returnValue = '';
}

function removeBeforeunload() {
    window.removeEventListener('beforeunload', beforeunload);
}