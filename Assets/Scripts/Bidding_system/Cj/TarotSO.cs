using UnityEngine;

[CreateAssetMenu(fileName = "New Tarot", menuName = "Tarot")]
public class TarotSO : ScriptableObject
{
    public string tarotName;
    public GameObject tarotPrefab;
    public string description;
    public Sprite UIAsset;
    public TarotObject.cardType type;
}