var GLOBAL = {};
var audioFileObjectURL = null;
GLOBAL.DotNetReference = null;
GLOBAL.SetDotNetReference = function (dotNetReference) {
    if (GLOBAL.DotNetReference === null) {
        GLOBAL.DotNetReference = dotNetReference;
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

//TODO: Remove this handling, when ASP.CORE 6 has global exception handling (https://github.com/dotnet/aspnetcore/issues/13452)
function removeBrowserHistoryEntry() {
    window.history.replaceState({}, 'ErrorReport', '/');
}

function reportError(error) {
    if (GLOBAL.DotNetReference !== null) {
        GLOBAL.DotNetReference.invokeMethodAsync("NotifyError", error);
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