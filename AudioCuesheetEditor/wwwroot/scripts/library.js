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