var fileObjectURL = null;
window.addEventListener('beforeunload', beforeunload);

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

function resetLocalStorage() {
    localStorage.clear();
}

window.AppSettings = {
    get: (key) => localStorage[key],
    set: (key, value) => localStorage[key] = value
};

function beforeunload(e) {
    e.preventDefault();
    e.returnValue = '';
}

function removeBeforeunload() {
    window.removeEventListener('beforeunload', beforeunload);
}