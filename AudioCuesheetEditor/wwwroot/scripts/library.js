window.addEventListener('beforeunload', beforeunload);

window.getObjectURLFromMudFileUpload = function (inputOrFieldId) {
    if (window._mudFileObjectURL) {
        try { URL.revokeObjectURL(window._mudFileObjectURL); } catch (e) { /* ignore */ }
        window._mudFileObjectURL = null;
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
            window._mudFileObjectURL = URL.createObjectURL(f);
            return window._mudFileObjectURL;
        }
    }
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