using UnityEngine;

public class AmbienceSceneDriver : MonoBehaviour
{
    private void Start()
    {
        AudioManager.Instance?.StartAmbience();
    }

    private void OnDestroy()
    {
        AudioManager.Instance?.StopAmbience(false);
    }
}
