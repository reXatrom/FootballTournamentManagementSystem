const storageKey = "ftms.musicPlayer.v3";
const DEBUG = false;
const defaultVolume = 0.7;

let audio = null;
let tracks = [];
let shuffleBag = [];
let currentIndex = -1;
let lastPlayedIndex = -1;
let unavailableTracks = new Set();
let dotNetReference = null;
let gestureHandler = null;
let autoplayBlocked = false;
let saveTimer = null;
let playerError = "";
let mediaSessionTrackIndex = -1;
let mediaSessionActionsRegistered = false;
let userPaused = false;
let initializationPromise = null;
let circuitUnavailable = false;

function debug(message, details = undefined) {
    if (DEBUG) console.debug(`[music] ${message}`, details ?? "");
}

function clampVolume(value) {
    return typeof value === "number" && Number.isFinite(value)
        ? Math.min(1, Math.max(0, value))
        : defaultVolume;
}

function readSavedState() {
    try {
        return JSON.parse(localStorage.getItem(storageKey) || "null");
    } catch {
        return null;
    }
}

function saveState() {
    if (!audio) return;

    const state = {
        trackIndex: currentIndex,
        currentTime: Number.isFinite(audio.currentTime) ? audio.currentTime : 0,
        volume: audio.volume,
        muted: audio.muted,
        paused: userPaused,
        shuffleBag,
        lastPlayedIndex
    };

    try {
        localStorage.setItem(storageKey, JSON.stringify(state));
    } catch (error) {
        console.warn("Unable to save music player state.", error);
    }
}

function getState() {
    const track = tracks[currentIndex];
    return {
        title: track?.title || "No track available",
        artist: track?.artist || "",
        isPlaying: Boolean(audio && !audio.paused),
        isPaused: !audio || audio.paused,
        volume: audio?.volume ?? 0.65,
        isMuted: audio?.muted ?? false,
        autoplayBlocked,
        hasTrack: Boolean(track),
        trackNumber: currentIndex < 0 ? 0 : currentIndex + 1,
        trackCount: tracks.length,
        currentTime: Number.isFinite(audio?.currentTime) ? audio.currentTime : 0,
        duration: Number.isFinite(audio?.duration) ? audio.duration : 0,
        errorMessage: playerError
    };
}

function notifyBlazor() {
    if (!dotNetReference || circuitUnavailable) return;
    updateMediaSession();
    const currentReference = dotNetReference;
    currentReference.invokeMethodAsync("OnMusicPlayerChanged", getState()).catch(error => {
        if (currentReference !== dotNetReference) return;
        debug("Blazor circuit callback failed; stopping audio", error?.message);
        circuitUnavailable = true;
        stopAndReset();
        dotNetReference = null;
    });
}

function handleOffline() {
    if (!audio) return;
    userPaused = true;
    autoplayBlocked = false;
    playerError = "Music stopped while offline. Press Play after reconnecting.";
    audio.pause();
    debug("offline; playback stopped");
    saveState();
    notifyBlazor();
}

function handleOnline() {
    if (playerError.startsWith("Music stopped while offline.")) {
        playerError = "Online again. Press Play to resume.";
        debug("online; waiting for user to resume playback");
        notifyBlazor();
    }
}

function updateMediaSession() {
    const session = navigator.mediaSession;
    if (!session) return;

    const track = tracks[currentIndex];
    if (track && mediaSessionTrackIndex !== currentIndex && typeof MediaMetadata !== "undefined") {
        session.metadata = new MediaMetadata({
            title: track.title,
            artist: track.artist,
            album: "Football Tournament Management"
        });
        mediaSessionTrackIndex = currentIndex;
    }

    if (!mediaSessionActionsRegistered) {
        for (const [action, handler] of [
            ["play", () => void resume()],
            ["pause", () => pause()],
            ["nexttrack", () => void next()]
        ]) {
            try {
                session.setActionHandler(action, handler);
            } catch {
                debug("media session action unsupported", action);
            }
        }
        mediaSessionActionsRegistered = true;
    }

    if (Number.isFinite(audio?.duration) && audio.duration > 0 && typeof session.setPositionState === "function") {
        try {
            session.setPositionState({
                duration: audio.duration,
                playbackRate: audio.playbackRate || 1,
                position: Math.min(audio.currentTime || 0, audio.duration)
            });
        } catch {
            debug("media session position update rejected");
        }
    }
}

function shuffle(values) {
    for (let index = values.length - 1; index > 0; index--) {
        const randomIndex = Math.floor(Math.random() * (index + 1));
        [values[index], values[randomIndex]] = [values[randomIndex], values[index]];
    }
    return values;
}

function refillShuffleBag() {
    shuffleBag = shuffle(tracks
        .map((_, index) => index)
        .filter(index => !unavailableTracks.has(index)));

    if (shuffleBag.length > 1 && shuffleBag[0] === lastPlayedIndex) {
        [shuffleBag[0], shuffleBag[1]] = [shuffleBag[1], shuffleBag[0]];
    }
}

function takeNextTrack() {
    if (shuffleBag.length === 0) refillShuffleBag();
    return shuffleBag.shift() ?? -1;
}

function removeGestureHandlers() {
    if (!gestureHandler) return;
    document.removeEventListener("pointerdown", gestureHandler, true);
    document.removeEventListener("keydown", gestureHandler, true);
    document.removeEventListener("touchstart", gestureHandler, true);
    gestureHandler = null;
}

function waitForUserGesture() {
    if (gestureHandler) return;
    autoplayBlocked = true;
    debug("autoplay blocked; waiting for a user gesture");
    gestureHandler = () => {
        removeGestureHandlers();
        autoplayBlocked = false;
        debug("first user gesture received; retrying play");
        void tryPlay();
    };
    document.addEventListener("pointerdown", gestureHandler, { once: true, capture: true });
    document.addEventListener("keydown", gestureHandler, { once: true, capture: true });
    document.addEventListener("touchstart", gestureHandler, { once: true, capture: true });
    notifyBlazor();
}

function setTrack(index, resumeTime = 0) {
    if (!audio || index < 0 || index >= tracks.length) return;

    currentIndex = index;
    playerError = "";
    audio.pause();
    audio.currentTime = 0;
    audio.src = tracks[index].src;
    audio.preload = "metadata";
    audio.load();
    debug("load", { src: audio.src, track: tracks[index] });

    if (resumeTime > 0) {
        const onMetadata = () => {
            if (currentIndex === index && Number.isFinite(audio.duration)) {
                audio.currentTime = Math.min(resumeTime, Math.max(0, audio.duration - 0.25));
                saveState();
            }
        };
        audio.addEventListener("loadedmetadata", onMetadata, { once: true });
    }

    saveState();
    notifyBlazor();
}

async function tryPlay() {
    if (!audio || currentIndex < 0) return;
    if (!navigator.onLine) {
        handleOffline();
        return;
    }

    try {
        await audio.play();
        autoplayBlocked = false;
        removeGestureHandlers();
        saveState();
        debug("play resolved", { volume: audio.volume, muted: audio.muted, paused: audio.paused });
        notifyBlazor();
    } catch (error) {
        debug("play rejected", { name: error?.name, message: error?.message });
        if (error?.name === "NotAllowedError") {
            waitForUserGesture();
            return;
        }

        if (error?.name === "AbortError") {
            debug("play interrupted while advancing tracks");
            return;
        }

        if (error?.name === "NotSupportedError") {
            markCurrentTrackUnavailable("unsupported media");
            return;
        }

        console.warn("Unable to play the current local audio track.", error);
        notifyBlazor();
    }
}

function markCurrentTrackUnavailable(reason) {
    if (currentIndex < 0 || unavailableTracks.has(currentIndex)) return;
    const failingSrc = tracks[currentIndex]?.src;
    console.warn(`Skipping unavailable music file: ${failingSrc} (${reason}).`);
    debug("error", { src: failingSrc, reason });
    unavailableTracks.add(currentIndex);
    shuffleBag = shuffleBag.filter(index => index !== currentIndex);
    void playNext(true);
}

async function playNext(shouldPlay) {
    if (!audio || tracks.length === 0) return;
    if (!navigator.onLine) {
        handleOffline();
        return;
    }
    if (shouldPlay) userPaused = false;

    const nextIndex = takeNextTrack();
    if (nextIndex < 0) {
        audio.pause();
        autoplayBlocked = false;
        playerError = "No playable audio found in /wwwroot/audio";
        debug("all tracks failed", { unavailableTracks: [...unavailableTracks] });
        notifyBlazor();
        saveState();
        return;
    }

    lastPlayedIndex = currentIndex;
    setTrack(nextIndex);
    if (shouldPlay) await tryPlay();
}

function restoreBag(saved) {
    if (!Array.isArray(saved?.shuffleBag)) return;
    shuffleBag = saved.shuffleBag.filter(index => Number.isInteger(index)
        && index >= 0 && index < tracks.length && index !== currentIndex);
}

export async function init(reference) {
    if (reference) {
        dotNetReference = reference;
        circuitUnavailable = false;
    }
    if (audio) {
        notifyBlazor();
        return getState();
    }

    if (initializationPromise) {
        await initializationPromise;
        if (reference) dotNetReference = reference;
        notifyBlazor();
        return getState();
    }

    initializationPromise = initializeAudio();
    try {
        return await initializationPromise;
    } finally {
        initializationPromise = null;
    }
}

async function initializeAudio() {
    try {
        const response = await fetch("/audio/playlist.json", { cache: "no-cache" });
        if (!response.ok) throw new Error(`Playlist request failed: ${response.status}`);
        const playlist = await response.json();
        tracks = (Array.isArray(playlist) ? playlist : playlist.tracks || [])
            .filter(track => {
                if (!track || typeof track.title !== "string" || typeof track.artist !== "string" || typeof track.src !== "string") return false;
                try {
                    const source = new URL(track.src, location.origin);
                    return source.origin === location.origin && source.pathname.startsWith("/audio/");
                } catch {
                    return false;
                }
            });
    } catch (error) {
        console.warn("Unable to load the local music playlist.", error);
        playerError = "Unable to load the local playlist.";
        notifyBlazor();
        return getState();
    }

    audio = new Audio();
    audio.preload = "metadata";
    audio.addEventListener("ended", () => { debug("ended", { src: audio.src }); void playNext(true); });
    audio.addEventListener("error", () => markCurrentTrackUnavailable(audio.error?.message || `media error ${audio.error?.code ?? "unknown"}`));
    audio.addEventListener("play", () => { debug("play event", { src: audio.src }); notifyBlazor(); saveState(); });
    audio.addEventListener("pause", () => { debug("pause", { src: audio.src }); notifyBlazor(); saveState(); });
    audio.addEventListener("volumechange", () => { debug("volume change", { volume: audio.volume, muted: audio.muted }); notifyBlazor(); saveState(); });
    audio.addEventListener("timeupdate", notifyBlazor);
    window.addEventListener("pagehide", saveState);
    window.addEventListener("offline", handleOffline);
    window.addEventListener("online", handleOnline);
    saveTimer = window.setInterval(saveState, 2000);

    if (tracks.length === 0) {
        console.warn("The local music playlist has no valid tracks.");
        playerError = "No playable audio found in /wwwroot/audio";
        notifyBlazor();
        return getState();
    }

    const saved = readSavedState();
    userPaused = saved?.paused === true;
    audio.volume = typeof saved?.volume === "number" && Number.isFinite(saved.volume)
        ? clampVolume(saved.volume)
        : defaultVolume;
    audio.muted = false;
    debug("init", { volume: audio.volume, muted: audio.muted, savedPaused: saved?.paused });

    if (Number.isInteger(saved?.trackIndex) && saved.trackIndex >= 0 && saved.trackIndex < tracks.length) {
        currentIndex = saved.trackIndex;
        lastPlayedIndex = Number.isInteger(saved.lastPlayedIndex) && saved.lastPlayedIndex >= 0 && saved.lastPlayedIndex < tracks.length
            ? saved.lastPlayedIndex
            : currentIndex;
        restoreBag(saved);
        const resumeTime = typeof saved.currentTime === "number" && Number.isFinite(saved.currentTime) && saved.currentTime >= 0
            ? saved.currentTime
            : 0;
        setTrack(currentIndex, resumeTime);
        if (saved.paused !== true) await tryPlay();
    } else {
        await playNext(true);
    }

    return getState();
}

export function pause() {
    if (!audio) return;
    userPaused = true;
    audio.pause();
    debug("pause requested");
    saveState();
}

export function pauseForVisibility() {
    if (!audio || audio.paused) return;
    audio.pause();
    saveState();
}

export function stopAndReset() {
    if (!audio) return;
    userPaused = false;
    audio.pause();
    try {
        audio.currentTime = 0;
    } catch {
        debug("unable to reset audio position during unload");
    }
    debug("page unloading; audio stopped and reset");
    saveState();
}

export function getPlaybackState() {
    return {
        isPlaying: Boolean(audio && !audio.paused),
        userPaused
    };
}

export async function resume() {
    if (!audio) return;
    userPaused = false;
    if (currentIndex < 0) {
        await playNext(true);
        return;
    }
    await tryPlay();
}

export async function next() {
    debug("next requested");
    await playNext(true);
}

export function setVolume(value) {
    if (!audio) return;
    audio.volume = clampVolume(typeof value === "number" ? value : Number(value));
    debug("volume set", { volume: audio.volume });
    saveState();
}

export function toggleMute() {
    if (!audio) return;
    audio.muted = !audio.muted;
    debug("mute toggled", { muted: audio.muted });
    saveState();
}

export function destroy() {
    saveState();
    removeGestureHandlers();
    if (saveTimer !== null) window.clearInterval(saveTimer);
    window.removeEventListener("pagehide", saveState);
    window.removeEventListener("offline", handleOffline);
    window.removeEventListener("online", handleOnline);
    audio?.pause();
    if (audio) audio.src = "";
    audio = null;
    dotNetReference = null;
}

export function seek(value) {
    if (!audio || !Number.isFinite(audio.duration) || audio.duration <= 0) return;
    const seconds = Number(value);
    if (!Number.isFinite(seconds)) return;
    audio.currentTime = Math.min(audio.duration, Math.max(0, seconds));
    updateMediaSession();
    saveState();
}