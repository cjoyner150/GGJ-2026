using UnityEngine;

[CreateAssetMenu(fileName = "New Mask", menuName = "Bidding/Mask")]
public class Mask_SO : ScriptableObject
{
    [Header("Basic Info")]
    public string maskName = "New Mask";
    [TextArea(3, 5)]
    public string description = "Mask description here";
    
    [Header("Visuals")]
    public Sprite icon; // For UI icons/buttons
    public GameObject modelPrefab;
    
    [Header("Blessed Mask Materials (Optional)")]
    public Material blessedMaterial;
    public Color blessedTint = Color.yellow;
    
    [Header("Stats (Optional)")]
    public int rarity = 1;
    public bool isSpecial = false;
}