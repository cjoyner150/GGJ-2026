using UnityEngine;

public enum MaskType
{
    Cursed,
    Clean
}

[System.Serializable]
public class Mask
{
    public string maskName;  
    public MaskType type;
    public int doomLevel;
    public string description;
    public GameObject modelPrefab;
    public Sprite icon; 
    public int value;
    
    // Optional additional properties
    public bool isSpecial = false;
    public Color tintColor = Color.white;
    public AudioClip revealSound;
    
    // Constructor for easier creation
    public Mask(string name, MaskType maskType, int doom, string desc, GameObject prefab = null)
    {
        maskName = name;
        type = maskType;
        doomLevel = doom;
        description = desc;
        modelPrefab = prefab;
    }
    
    public string DisplayName => !string.IsNullOrEmpty(maskName) ? maskName : $"Mask {type}";
    
    public Color TypeColor
    {
        get
        {
            return type switch
            {
                MaskType.Cursed => Color.red,
                MaskType.Clean => Color.green,
                _ => Color.white
            };
        }
    }
}