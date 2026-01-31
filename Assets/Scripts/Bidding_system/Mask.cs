public enum MaskType
{
    Cursed,
    Clean
}

[System.Serializable]
public class Mask
{
    public MaskType type;
    public int doomLevel;
    public string description;
}
