using UnityEngine;
using FMODUnity;

[CreateAssetMenu(menuName = "Audio/Audio Events")]
public class AudioEvents : ScriptableObject
{
    [Header("UI")]
    public EventReference uiClick;
    public EventReference cardPickup;

    [Header("Player - Movement")]
    public EventReference playerFootstep;

    [Header("Gameplay")]
    public EventReference playerPunch;

    [Header("Music")]
    public EventReference music;

    // [Header("Ambience")]
    public EventReference ambient;

    [Header("Voices")]
    public EventReference voiceEnd;
    public EventReference voiceFight;

}
