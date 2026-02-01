using UnityEngine;

[System.Serializable]
public class MaskObject
{
    public string name;
    public Effector maskEffector;
    public GameObject maskPrefab;
    public string maskDescription;
    public int maskDoom;

    public MaskObject(MaskSO maskSO)
    {
        name = maskSO.maskName;
        maskPrefab = maskSO.maskPrefab;
        maskDescription = maskSO.maskDescription;
        maskDoom = maskSO.maskDoom;
    }
}
