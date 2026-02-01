using UnityEngine;

[CreateAssetMenu(fileName ="New Mask", menuName = "Mask")]
public class MaskSO : ScriptableObject
{
    public string maskName;
    public Effector effector;
    public GameObject maskPrefab;
    public string maskDescription;
    public int maskDoom;

    public MaskObject.maskType type;
}
