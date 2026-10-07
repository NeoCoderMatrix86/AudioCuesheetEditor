window.addEventListener('beforeunload', beforeunload);

window.getObjectURLFromMudFileUpload = function (fileName, fileSize, fileContentType, fileLastModified) {
    const inputElement = document.querySelector(`input.file-upload-input`);

    const files = inputElement.files;
    for (let i = 0; i < files.length; i++) {
        const file = files[i];

        if (file &&
            file.type &&
            file.type.startsWith("audio/") &&
            file.name === fileName &&
            file.size === fileSize &&
            file.type === fileContentType &&
            file.lastModified === fileLastModified) {

            return URL.createObjectURL(file);
        }
    }

    return null;
};

window.revokeAudioObjectURL = function (objectUrl) {
    URL.revokeObjectURL(objectUrl);
};

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

function getAudioDurationFromFile(url) {
    return new Promise((resolve, reject) => {
        const audio = new Audio();

        audio.preload = "metadata";
        audio.src = url;

        audio.onloadedmetadata = () => {
            resolve(audio.duration);
        };

        audio.onerror = (e) => {
            reject(e);
        };
    });
}