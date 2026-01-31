using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MockPlayerInput : MonoBehaviour
{
    [Header("Player References")]
    public List<PlayerGold> players;
    
    [Header("Controller References")]
    public MaskBiddingController biddingController;
    public BidChooser bidChooser;
    
    [Header("Input Settings")]
    public bool enableKeyboardInput = true;
    public Key[] playerBidKeys = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4 };
    public Key increaseBidKey = Key.UpArrow;
    public Key decreaseBidKey = Key.DownArrow;
    public Key confirmBidKey = Key.Space;
    public Key nextPlayerKey = Key.N;
    public Key drawMaskKey = Key.M;
    public Key drawTarotKey = Key.T;
    
    [Header("Auto-Bid Settings")]
    public bool autoBidEnabled = false;
    public float autoBidDelay = 2f;
    private float[] playerAutoBidTimers;
    
    private Keyboard keyboard;
    private bool isInitialized = false;
    
    void Start()
    {
        keyboard = Keyboard.current;
        
        // Try to find components if not assigned
        if (players == null || players.Count == 0)
        {
            players = new List<PlayerGold>(FindObjectsOfType<PlayerGold>());
        }
        
        if (biddingController == null)
            biddingController = FindObjectOfType<MaskBiddingController>();
            
        if (bidChooser == null)
            bidChooser = FindObjectOfType<BidChooser>();
        
        // Initialize auto-bid timers
        if (players.Count > 0)
        {
            playerAutoBidTimers = new float[players.Count];
            for (int i = 0; i < playerAutoBidTimers.Length; i++)
            {
                playerAutoBidTimers[i] = autoBidDelay;
            }
        }
        
        isInitialized = true;
        
        Debug.Log($"MockPlayerInput ready. Players: {players.Count}");
        Debug.Log("Controls: 1-4=Bid, ↑/↓=Adjust, Space=Confirm");
        Debug.Log("N=Next Player, M=Draw Mask, T=Draw Tarot");
    }
    
    void Update()
    {
        if (!isInitialized || keyboard == null) return;
        
        if (enableKeyboardInput)
        {
            HandleKeyboardInput();
        }
        
        if (autoBidEnabled && biddingController != null && biddingController.IsMaskPhase)
        {
            UpdateAutoBidding();
        }
    }
    
    void HandleKeyboardInput()
    {
        // Number keys 1-4: Bid for specific players
        for (int i = 0; i < Mathf.Min(playerBidKeys.Length, players.Count); i++)
        {
            if (keyboard[playerBidKeys[i]].wasPressedThisFrame)
            {
                SimulatePlayerBid(players[i]);
            }
        }
        
        // Bid adjustment (if bid chooser is active)
        if (bidChooser != null && bidChooser.gameObject.activeSelf)
        {
            if (keyboard[increaseBidKey].wasPressedThisFrame)
            {
                bidChooser.IncreaseBid();
            }
            
            if (keyboard[decreaseBidKey].wasPressedThisFrame)
            {
                bidChooser.DecreaseBid();
            }
            
            if (keyboard[confirmBidKey].wasPressedThisFrame)
            {
                bidChooser.ConfirmBid();
            }
        }
        
        // Quick navigation
        if (keyboard[nextPlayerKey].wasPressedThisFrame && bidChooser != null)
        {
            bidChooser.MoveToNextPlayer();
        }
        
        // Test functions
        if (keyboard[drawMaskKey].wasPressedThisFrame && biddingController != null)
        {
            biddingController.TestDrawMask();
        }
        
        if (keyboard[drawTarotKey].wasPressedThisFrame && biddingController != null)
        {
            biddingController.TestDrawTarot();
        }
        
        // Quick add to pot
        if (keyboard.pKey.wasPressedThisFrame && biddingController != null)
        {
            biddingController.AddToPot(Random.Range(10, 51));
        }
        
        // Switch phases
        if (keyboard.fKey.wasPressedThisFrame && biddingController != null)
        {
            if (biddingController.IsMaskPhase)
                biddingController.BeginTarotPhase();
            else
                biddingController.BeginMaskPhase();
        }
    }
    
    void UpdateAutoBidding()
    {
        for (int i = 0; i < players.Count; i++)
        {
            playerAutoBidTimers[i] -= Time.deltaTime;
            
            if (playerAutoBidTimers[i] <= 0)
            {
                SimulatePlayerBid(players[i]);
                playerAutoBidTimers[i] = autoBidDelay + Random.Range(-0.5f, 0.5f);
            }
        }
    }
    
    void SimulatePlayerBid(PlayerGold player)
    {
        if (player == null || biddingController == null) return;
        
        Debug.Log($"Mock: Player {player.PlayerIndex} bidding");
        
        // If bid chooser is active, use it
        if (bidChooser != null && bidChooser.gameObject.activeSelf)
        {
            // Find which player index this is
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] == player)
                {
                    // Set bid chooser to this player
                    bidChooser.MovePanelToPlayer(i);
                    // Confirm bid with current amount
                    bidChooser.ConfirmBid();
                    break;
                }
            }
        }
        else
        {
            // Direct bid through controller
            biddingController.PlayerBid(player);
        }
    }
    
    // Public methods for UI buttons
    public void TestBidAllPlayers()
    {
        if (!isInitialized) return;
        
        foreach (var player in players)
        {
            SimulatePlayerBid(player);
        }
    }
    
    public void TestRandomBid()
    {
        if (!isInitialized || players.Count == 0) return;
        
        int randomIndex = Random.Range(0, players.Count);
        SimulatePlayerBid(players[randomIndex]);
    }
    
    public void ToggleAutoBid()
    {
        autoBidEnabled = !autoBidEnabled;
        Debug.Log($"Auto-bid: {autoBidEnabled}");
    }
}