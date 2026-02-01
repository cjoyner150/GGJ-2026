using UnityEngine;

[System.Serializable]
public class MaskObject
{
    public enum maskType
    {
        Bat,
        Bear,
        Bee,
        Butterfly,
        Crow,
        Man,
        Rabbit,
        Snake,
        Turtle,
        Goddess
    }

    public string name;
    public Effector maskEffector;
    public GameObject maskPrefab;
    public string maskDescription;
    public int maskDoom;
    public maskType type;

    public MaskObject(MaskSO maskSO)
    {
        name = maskSO.maskName;
        maskPrefab = maskSO.maskPrefab;
        maskDescription = maskSO.maskDescription;
        maskDoom = maskSO.maskDoom;
        type = maskSO.type;
    }
}
