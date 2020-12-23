var GLOBAL = {};
var audioFileObjectURL = null;
GLOBAL.DotNetReference = null;
GLOBAL.SetDotNetReference = function (dotNetReference) {
    if (GLOBAL.DotNetReference === null) {
        GLOBAL.DotNetReference = dotNetReference;
    }
};
function audioFileChanged(eventSrc) {
    if (audioFileObjectURL != null) {
        URL.revokeObjectURL(audioFileObjectURL);
    }
    audioFileObjectURL = URL.createObjectURL(eventSrc.files[0]);
    GLOBAL.DotNetReference.invokeMethodAsync("AudioFileChanged", eventSrc.files[0].name, audioFileObjectURL);
}
window.blazorCulture = {
    get: () => localStorage['BlazorCulture'],
    set: (value) => localStorage['BlazorCulture'] = value
};
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