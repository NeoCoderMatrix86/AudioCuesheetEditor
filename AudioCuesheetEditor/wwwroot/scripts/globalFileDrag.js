window.globalFileDrag = (function () {
    let dotnetRef = null;
    let counter = 0;
    let registered = false;

    function hasFiles(e) {
        try {
            const dt = e.dataTransfer || e;
            if (!dt || !dt.types) return false;
            for (let i = 0; i < dt.types.length; i++) {
                if (dt.types[i] === "Files") return true;
            }
        } catch { }
        return false;
    }

    function safeInvoke(methodName) {
        if (!registered || !dotnetRef || typeof dotnetRef.invokeMethodAsync !== "function") return;

        Promise.resolve()
            .then(() => dotnetRef.invokeMethodAsync(methodName))
            .catch(function (err) {
                console.debug("globalFileDrag: invokeMethodAsync rejected:", err && err.message ? err.message : err);
            });
    }

    function onDragEnter(e) {
        if (!hasFiles(e)) return;
        counter++;
        if (counter === 1) {
            safeInvoke('OnGlobalDragEnter');
        }
    }

    function onDragLeave(e) {
        counter--;
        if (counter <= 0) {
            counter = 0;
            safeInvoke('OnGlobalDragLeave');
        }
    }

    function register(dotnetObject) {        
        dotnetRef = dotnetObject;
        registered = true;
        counter = 0;

        document.addEventListener('dragenter', onDragEnter, false);
        document.addEventListener('dragleave', onDragLeave, false);
    }

    function unregister() {
        document.removeEventListener('dragenter', onDragEnter, false);
        document.removeEventListener('dragleave', onDragLeave, false);

        registered = false;
        dotnetRef = null;
        counter = 0;
    }

    function reset() {
        counter = 0;
    }

    return {
        register: register,
        unregister: unregister,
        reset: reset
    };
})();