using UnityEngine;
using FMODUnity;

[CreateAssetMenu(menuName = "Audio/Audio Events")]
public class AudioEvents : ScriptableObject
{
    [Header("UI")]
    // public EventReference uiClick;

    [Header("Player - Movement")]
    public EventReference playerFootstep;

    [Header("Music")]
    public EventReference music;

    // [Header("Ambience")]
    public EventReference ambient;

    [Header("Gameplay")]
    public EventReference cardPickup;

}
