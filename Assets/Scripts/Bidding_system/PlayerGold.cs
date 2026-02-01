using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class PlayerGold : MonoBehaviour
{
    [Header("Player Info")]
    public int Gold = 100;
    public int startingGold = 100;
    
    public int PlayerIndex { get; private set; }
    public Color PlayerColor { get; private set; }
    
    public event Action<int> OnGoldChanged;
    
    private static readonly Color[] PLAYER_COLORS = {
        new Color(1f, 0.3f, 0.3f),    // Red
        new Color(0.3f, 0.3f, 1f),    // Blue
        new Color(0.3f, 0.8f, 0.3f),  // Green
        new Color(1f, 0.8f, 0.3f),    // Yellow
        new Color(1f, 0.3f, 1f),      // Purple
        new Color(0.3f, 1f, 1f)       // Cyan
    };

    void Awake()
    {
        InitializeComponent();
    }

    void Start()
    {
        InitializeFromPlayerConfig();
    }

    private void InitializeComponent()
    {
        // Try multiple ways to get player index
        PlayerIndex = GetPlayerIndexFromComponents();
        
        // Default color
        PlayerColor = PlayerIndex < PLAYER_COLORS.Length ? 
            PLAYER_COLORS[PlayerIndex] : Color.white;
        
        Gold = startingGold;
        
        Debug.Log($"PlayerGold: Player {PlayerIndex} initialized with {Gold}G, Color: {PlayerColor}");
    }

    private int GetPlayerIndexFromComponents()
    {
        // Try PlayerInputHandler first
        PlayerInputHandler inputHandler = GetComponent<PlayerInputHandler>();
        if (inputHandler != null)
        {
            return GetIndexFromInputHandler(inputHandler);
        }
        
        // Try PlayerInput
        PlayerInput playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            return playerInput.playerIndex;
        }
        
        // Fallback to sibling index
        return transform.GetSiblingIndex();
    }
    
    private int GetIndexFromInputHandler(PlayerInputHandler handler)
    {
        // Try to get index via reflection if we can't modify PlayerInputHandler
        System.Type type = handler.GetType();
        var property = type.GetProperty("PlayerIndex");
        if (property != null)
        {
            return (int)property.GetValue(handler);
        }
        
        var field = type.GetField("PlayerIndex");
        if (field != null)
        {
            return (int)field.GetValue(handler);
        }
        
        // If no PlayerIndex, use default
        return transform.GetSiblingIndex();
    }

    private void InitializeFromPlayerConfig()
    {
        // Try to get color from PlayerConfigManager
        var configs = PlayerConfigManager.Instance?.GetPlayerConfigs();
        if (configs != null && PlayerIndex < configs.Count)
        {
            PlayerColor = configs[PlayerIndex].PlayerColor;
            Debug.Log($"Player {PlayerIndex} got color from PlayerConfigManager: {PlayerColor}");
        }
    }
    
    public void SetPlayerIndex(int index)
    {
        PlayerIndex = index;
        PlayerColor = PlayerIndex < PLAYER_COLORS.Length ? 
            PLAYER_COLORS[PlayerIndex] : Color.white;
        Debug.Log($"Player index set to {index}");
    }
    
    public void SetPlayerColor(Color color)
    {
        PlayerColor = color;
    }
    
    public bool CanAfford(int amount) => Gold >= amount;
    
    public bool TrySpend(int amount)
    {
        if (!CanAfford(amount)) 
        {
            Debug.Log($"Player {PlayerIndex} cannot afford {amount}G (has {Gold}G)");
            return false;
        }
        
        Gold -= amount;
        Debug.Log($"Player {PlayerIndex} spent {amount}G (remaining: {Gold}G)");
        OnGoldChanged?.Invoke(Gold);
        return true;
    }
    
    public void Add(int amount)
    {
        Gold += amount;
        Debug.Log($"Player {PlayerIndex} gained {amount}G (total: {Gold}G)");
        OnGoldChanged?.Invoke(Gold);
    }
    
    public void SetGold(int amount)
    {
        Gold = Mathf.Max(0, amount);
        Debug.Log($"Player {PlayerIndex} gold set to {Gold}G");
        OnGoldChanged?.Invoke(Gold);
    }
    
    public void ResetGold(int startingGold = 100)
    {
        SetGold(startingGold);
    }
}