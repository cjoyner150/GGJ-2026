using UnityEngine;
using FMODUnity;

[CreateAssetMenu(menuName = "Audio/Audio Events")]
public class AudioEvents : ScriptableObject
{
    [Header("UI")]
    public EventReference uiClick;
    public EventReference cardPickup;
    public EventReference uiJoin;
    public EventReference uiReady;
    public EventReference uiTickUp;
    public EventReference uiTickDown;
    public EventReference uiBet;
    public EventReference uiTake;

    [Header("Player - Movement")]
    public EventReference playerFootstep;

    [Header("Gameplay")]
    public EventReference playerPunch;
    public EventReference playerAttack;
    public EventReference playerJump;
    public EventReference playerDash;
    public EventReference playerParry;

    [Header("Music")]
    public EventReference music;

    // [Header("Ambience")]
    public EventReference ambient;

    [Header("Voices")]
    public EventReference voiceEnd;
    public EventReference voiceFight;

}
