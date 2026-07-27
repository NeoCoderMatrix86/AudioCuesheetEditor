window.addEventListener('beforeunload', beforeunload);

window._audioObjectURLCache = {};
window.getObjectURLFromMudFileUpload = function (inputOrFieldId) {
    if (window._audioObjectURLCache[inputOrFieldId]) {
        return window._audioObjectURLCache[inputOrFieldId];
    }

    let inputElem = inputOrFieldId;
    if (typeof inputOrFieldId === "string") {
        inputElem = document.getElementById(inputOrFieldId) ||
            document.querySelector(`input[identifier="${inputOrFieldId}"]`) ||
            document.querySelector(`input[id="${inputOrFieldId}"]`);
    }

    const files = inputElem.files;
    for (let i = 0; i < files.length; i++) {
        const f = files[i];
        if (f && f.type && f.type.startsWith("audio/")) {
            const newObjectURL = URL.createObjectURL(f);
            window._audioObjectURLCache[inputOrFieldId] = newObjectURL;
            return newObjectURL;
        }
    }
    return null;
};

window.revokeAudioObjectURL = function (objectUrl) {
    Object.keys(window._audioObjectURLCache).forEach(key => {
        if (window._audioObjectURLCache[key] === objectUrl) {
            delete window._audioObjectURLCache[key];
        }
    });
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