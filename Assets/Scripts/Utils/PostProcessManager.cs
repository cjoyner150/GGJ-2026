
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // Use for URP. For HDRP, use UnityEngine.Rendering.HighDefinition;

public class RuntimeHueShift : MonoBehaviour
{
    public Volume volume; // Drag your Volume GameObject here in the Inspector
    private ColorAdjustments colorAdjustments;
    public bool colorshift;
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
    void Start()
    {
        // Try to get the ColorAdjustments effect from the volume profile
        if (volume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
        {
            Debug.Log("Color Adjustments found in profile.");
            if (colorshift)
            {
                colorAdjustments.hueShift.value = Random.Range(-180, 180);
            }
            
        }
        else
        {
            Debug.LogError("No ColorAdjustments found on the volume profile.");
        }
    }

    void Update()
    {
        
    }
}