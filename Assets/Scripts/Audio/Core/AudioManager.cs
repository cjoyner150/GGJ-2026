using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private AudioEvents events;

    private EventInstance musicInstance;
    private EventInstance ambientInstance;

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

    private void Start()
    {
        if (events == null)
        {
            Debug.LogError("[AudioManager] AudioEvents is not assigned.");
            return;
        }

        // StartAmbience();
        StartMusic();
    }

    private void OnDestroy()
    {
        if (Instance != this) return;

        StopMusic(immediate: true);
        StopAmbience(immediate: true);

        Instance = null;
    }

    // ---------------------------
    // One-shots
    // ---------------------------
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

    // Wrapper using AudioEvents

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



    // ---------------------------
    // Music
    // ---------------------------
    public void StartMusic()
    {
        if (events.music.IsNull) return;

        if (musicInstance.isValid()) return;

        musicInstance = RuntimeManager.CreateInstance(events.music);
        musicInstance.start();
    }

    public void StopMusic(bool immediate = false)
    {
        if (!musicInstance.isValid()) return;

        musicInstance.stop(immediate ? STOP_MODE.IMMEDIATE : STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();
        musicInstance.clearHandle();
    }

    // ---------------------------
    // Ambience
    // ---------------------------
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
