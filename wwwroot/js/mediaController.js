let playerModule;
let startPromise;
let wasPlayingBeforeHidden = false;

async function getPlayer() {
    if (!playerModule) {
        playerModule = await import("./musicPlayer.js");
    }
    return playerModule;
}

async function startPlayback() {
    if (!startPromise) {
        startPromise = getPlayer().then(player => player.init(null));
    }

    try {
        await startPromise;
    } catch (error) {
        console.warn("Unable to initialize tournament music.", error);
    }
}

async function handleVisibilityChange() {
    try {
        const player = await getPlayer();
        if (document.hidden) {
            const state = player.getPlaybackState();
            wasPlayingBeforeHidden = state.isPlaying;
            if (wasPlayingBeforeHidden) {
                player.pauseForVisibility();
            }
            return;
        }

        if (wasPlayingBeforeHidden) {
            wasPlayingBeforeHidden = false;
            await player.resume();
        }
    } catch (error) {
        console.warn("Unable to manage tournament music visibility state.", error);
    }
}

function stopForPageExit() {
    if (!playerModule) return;
    void playerModule.then(player => player.stopAndReset()).catch(error => {
        console.warn("Unable to stop tournament music while leaving the page.", error);
    });
}

function onDocumentReady() {
    void startPlayback();
}

if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", onDocumentReady, { once: true });
} else {
    onDocumentReady();
}

document.addEventListener("visibilitychange", () => void handleVisibilityChange());
window.addEventListener("beforeunload", stopForPageExit);
window.addEventListener("pagehide", stopForPageExit);