using UnityEngine;

[System.Serializable]
public class TarotCard
{
    public string cardName;     
    public string name => cardName; 
    public string description;
    public bool isRare;       
    public GameObject modelPrefab;  
    public Sprite cardImage;     
    public string effect;         
    public int baseValue;       
    
    // Optional additional properties
    public TarotSuit suit = TarotSuit.Major;
    public int cardNumber = 0;
    public bool isReversible = false;
    public AudioClip drawSound;
    
    // Constructor for easier creation
    public TarotCard(string name, string desc, bool rare = false, string eff = "", GameObject prefab = null)
    {
        cardName = name;
        description = desc;
        isRare = rare;
        effect = eff;
        modelPrefab = prefab;
    }
    
    public string FullDescription => $"{cardName}: {description}\nEffect: {effect}";
    
    public Color RarityColor => isRare ? new Color(1f, 0.8f, 0f) : Color.white;
}

public enum TarotSuit
{
    Major,      
    Wands,      // Suit of Wands
    Cups,       // Suit of Cups
    Swords,     // Suit of Swords
    Pentacles   // Suit of Pentacles
}