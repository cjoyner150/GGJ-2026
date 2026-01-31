using UnityEngine;
using System;

public class PlayerGold : MonoBehaviour
{
    public int Gold = 100;
    public int PlayerIndex { get; private set; }
    public Color PlayerColor { get; private set; }
    
    public event Action<int> OnGoldChanged;
    
    void Start()
    {
        if (TryGetComponent<PlayerIdentifier>(out var identifier))
        {
            PlayerIndex = identifier.Index;
            PlayerColor = GetColorFromIndex(PlayerIndex);
        }
        else
        {
            PlayerIndex = transform.GetSiblingIndex();
            PlayerColor = GetColorFromIndex(PlayerIndex);
        }
    }
    
    private Color GetColorFromIndex(int index)
    {
        Color[] colors = {
            new Color(1f, 0.3f, 0.3f),    // Red
            new Color(0.3f, 0.3f, 1f),    // Blue
            new Color(0.3f, 0.8f, 0.3f),  // Green
            new Color(1f, 0.8f, 0.3f),    // Yellow
            new Color(1f, 0.3f, 1f),      // Purple
            new Color(0.3f, 1f, 1f),      // Cyan
        };
        
        return index < colors.Length ? colors[index] : Color.white;
    }
    
    public bool CanAfford(int amount) => Gold >= amount;
    
    public bool TrySpend(int amount)
    {
        if (!CanAfford(amount)) return false;
        
        Gold -= amount;
        OnGoldChanged?.Invoke(Gold);
        return true;
    }
    
    public void Add(int amount)
    {
        Gold += amount;
        OnGoldChanged?.Invoke(Gold);
    }
}