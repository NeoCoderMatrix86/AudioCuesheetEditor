var GLOBAL = {};
var audioFileObjectURL = null;
var startTime;
var mediaStream = null;
window.addEventListener('beforeunload', beforeunload);
GLOBAL.Index = null;
GLOBAL.ViewModeRecord = null;
GLOBAL.SetIndexReference = function (dotNetReference) {
    if (GLOBAL.Index === null) {
        GLOBAL.Index = dotNetReference;
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

window.ExportOptions = {
    get: () => localStorage['ExportOptions'],
    set: (value) => localStorage['ExportOptions'] = value
}

window.ImportOptions = {
    get: () => localStorage['ImportOptions'],
    set: (value) => localStorage['ImportOptions'] = value
}

window.RecordOptions = {
    get: () => localStorage['RecordOptions'],
    set: (value) => localStorage['RecordOptions'] = value
}

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
    mediaStream.getTracks().forEach(function (track) {
        track.stop();
    });
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

function getBoundingClientRect(domId) {
    var element = document.getElementById(domId);
    return element.getBoundingClientRect();
}