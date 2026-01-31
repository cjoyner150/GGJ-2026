using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MaskDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    public Image maskImage;
    public TMP_Text typeText;
    public TMP_Text doomText;
    public TMP_Text descriptionText;
    
    [Header("Sprites")]
    public Sprite cursedMaskSprite;
    public Sprite cleanMaskSprite;
    
    public void SetMask(Mask mask)
    {
        // Set mask type color and sprite
        if (mask.type == MaskType.Cursed)
        {
            typeText.text = "CURSED";
            typeText.color = Color.red;
            if (maskImage != null && cursedMaskSprite != null)
                maskImage.sprite = cursedMaskSprite;
        }
        else
        {
            typeText.text = "CLEAN";
            typeText.color = Color.green;
            if (maskImage != null && cleanMaskSprite != null)
                maskImage.sprite = cleanMaskSprite;
        }
        
        // Set doom level
        doomText.text = $"Doom: {mask.doomLevel}";
        doomText.color = Color.Lerp(Color.white, Color.red, mask.doomLevel / 10f);
        
        // Set description
        if (descriptionText != null)
            descriptionText.text = mask.description;
    }
}