var GLOBAL = {};
GLOBAL.DotNetReference = null;
GLOBAL.SetDotNetReference = function (dotNetReference) {
    if (GLOBAL.DotNetReference === null) {
        GLOBAL.DotNetReference = dotNetReference;
    }
};
function audioFileChanged(eventSrc) {
    GLOBAL.DotNetReference.invokeMethodAsync("AudioFileChanged", eventSrc.files[0].name);
}