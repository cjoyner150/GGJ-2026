using UnityEngine;

[CreateAssetMenu(fileName = "New Tarot", menuName = "Bidding/Tarot")]
public class TarotCard_SO : ScriptableObject
{
    [Header("Basic Info")]
    public string cardName = "New Tarot";
    [TextArea(3, 5)]
    public string effect = "Card effect description";
    
    [Header("Visuals")]
    public Sprite icon; // For UI icons/buttons
    public GameObject modelPrefab; // The actual 3D card model
    
    [Header("Rarity")]
    public bool isRare = false;
    public Material rareMaterial; // Special material for rare cards
    public Color rareGlow = new Color(0.5f, 0.2f, 1f); // Purple
    
    [Header("Stats (Optional)")]
    public int powerLevel = 1; // For balancing
    public string cardType = "Normal"; // "Attack", "Defense", "Special", etc.
}