using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;
using FMOD.Studio;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private AudioEvents events;

    [Header("Music Switching")]
    [SerializeField] private SceneMusicTable sceneMusicTable;
    [SerializeField] private bool switchMusicOnSceneLoad = true;

    private EventInstance musicInstance;
    private EventInstance ambientInstance;

    private EventReference currentMusicEvent;

    // --------------------------------------------------
    // Lifecycle
    // --------------------------------------------------
    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (events == null)
        {
            Debug.LogError("[AudioManager] AudioEvents is not assigned.");
            return;
        }

        // StartAmbience();

        // If you're switching by scene, let OnSceneLoaded handle it.
        // Otherwise fall back to your default events.music.
        if (!switchMusicOnSceneLoad)
            StartMusic();
    }

    private void OnDestroy()
    {
        if (Instance != this) return;

        StopMusic(immediate: true);
        StopAmbience(immediate: true);

        Instance = null;
    }

    // --------------------------------------------------
    // Scene switching
    // --------------------------------------------------
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!switchMusicOnSceneLoad) return;
        if (sceneMusicTable == null) return;

        if (sceneMusicTable.TryGet(scene.name, out var musicEvent) && !musicEvent.IsNull)
        {
            StartMusic(musicEvent);
        }
        else
        {
            // Optional fallback: if no mapping found, play default music (events.music)
            // StartMusic();

            // Or stop music if unmapped:
            // StopMusic(immediate: false);
        }
    }

    // --------------------------------------------------
    // One-shots (generic)
    // --------------------------------------------------
    public void PlayUI(EventReference evt)
    {
        if (evt.IsNull) return;
        RuntimeManager.PlayOneShot(evt);
    }

    public void PlayAt(EventReference evt, Vector3 position)
    {
        if (evt.IsNull) return;
        RuntimeManager.PlayOneShot(evt, position);
    }

    // --------------------------------------------------
    // One-shots (AudioEvents wrappers)
    // --------------------------------------------------

    // UI
    public void cardPickup() => PlayUI(events.cardPickup);
    public void uiClick() => PlayUI(events.uiClick);
    public void uiJoin() => PlayUI(events.uiJoin);
    public void uiReady() => PlayUI(events.uiReady);

    // Voice
    public void voiceEnd() => PlayUI(events.voiceEnd);
    public void voiceFight(Vector3 pos) => PlayAt(events.voiceFight, pos);

    // Gameplay
    public void playFootstep(Vector3 pos) => PlayAt(events.playerFootstep, pos);
    public void playPunch(Vector3 pos) => PlayAt(events.playerPunch, pos);
    public void playAttack(Vector3 pos) => PlayAt(events.playerAttack, pos);
    public void playJump(Vector3 pos) => PlayAt(events.playerJump, pos);
    public void playDash(Vector3 pos) => PlayAt(events.playerDash, pos);
    public void playParry(Vector3 pos) => PlayAt(events.playerParry, pos);

    // --------------------------------------------------
    // Music
    // --------------------------------------------------

    // Your original default music start (uses events.music)
    public void StartMusic()
    {
        if (events.music.IsNull) return;

        // If already playing that same default event, do nothing
        if (musicInstance.isValid() && currentMusicEvent.Guid == events.music.Guid)
            return;

        StartMusic(events.music);
    }

    // New overload: start/switch to a specific event
    public void StartMusic(EventReference musicEvent)
    {
        if (musicEvent.IsNull) return;

        // If same event already playing, do nothing
        if (musicInstance.isValid() && currentMusicEvent.Guid == musicEvent.Guid)
            return;

        // Stop previous (fade out)
        StopMusic(immediate: false);

        musicInstance = RuntimeManager.CreateInstance(musicEvent);
        musicInstance.start();

        currentMusicEvent = musicEvent;
    }

    public void StopMusic(bool immediate = false)
    {
        if (!musicInstance.isValid()) return;

        musicInstance.stop(immediate ? STOP_MODE.IMMEDIATE : STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();
        musicInstance.clearHandle();

        currentMusicEvent = default;
    }

    // --------------------------------------------------
    // Ambience
    // --------------------------------------------------
    public void StartAmbience()
    {
        if (events.ambient.IsNull) return;
        if (ambientInstance.isValid()) return;

        ambientInstance = RuntimeManager.CreateInstance(events.ambient);
        ambientInstance.start();
    }

    public void StopAmbience(bool immediate = false)
    {
        if (!ambientInstance.isValid()) return;

        ambientInstance.stop(immediate ? STOP_MODE.IMMEDIATE : STOP_MODE.ALLOWFADEOUT);
        ambientInstance.release();
        ambientInstance.clearHandle();
    }
}
