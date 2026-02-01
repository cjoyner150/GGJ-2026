using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MaskBiddingController biddingController;
    [SerializeField] private BidChooser bidChooser;
    
    [Header("Players")]
    private List<PlayerGold> players = new List<PlayerGold>();
    private List<PlayerBiddingInput> playerInputs = new List<PlayerBiddingInput>();
    private Dictionary<int, PlayerBiddingInput> playerInputMap = new Dictionary<int, PlayerBiddingInput>();
    
    private int currentTurnPlayerIndex = 0;
    private bool isTurnBased = true;
    private bool isBiddingActive = false;
    
    public void Initialize(List<PlayerGold> playerList, List<PlayerBiddingInput> inputList, 
                          MaskBiddingController controller, BidChooser chooser)
    {
        players = playerList;
        playerInputs = inputList;
        biddingController = controller;
        bidChooser = chooser;
        
        // Create map for quick lookup
        playerInputMap.Clear();
        foreach (var input in playerInputs)
        {
            playerInputMap[input.GetPlayerIndex()] = input;
        }
        
        Debug.Log($"TurnManager initialized with {players.Count} players");
    }
    
    public void RegisterPlayer(PlayerBiddingInput input, int playerIndex)
    {
        if (!playerInputMap.ContainsKey(playerIndex))
        {
            playerInputMap[playerIndex] = input;
            Debug.Log($"TurnManager registered Player {playerIndex}");
        }
    }
    
    public void StartMaskPhase()
    {
        Debug.Log("TurnManager: Starting Mask Phase");
        
        isBiddingActive = true;
        isTurnBased = true;
        
        // Reset all players
        foreach (var input in playerInputs)
        {
            input.ResetForNewRound();
            input.SetBiddingPhase(true);
            input.SetIsMyTurn(false);
            input.SetBiddingPhaseActive(true);
        }
        
        // Start with player 0
        SetCurrentPlayerTurn(0);
    }
    
    public void StartTarotPhase()
    {
        Debug.Log("TurnManager: Starting Tarot Phase");
        
        isBiddingActive = true;
        isTurnBased = true;
        
        // Reset all players
        foreach (var input in playerInputs)
        {
            input.ResetForNewRound();
            input.SetBiddingPhase(false);
            input.SetIsMyTurn(false);
            input.SetBiddingPhaseActive(true);
        }
        
        // Start with player 0
        SetCurrentPlayerTurn(0);
    }
    
    public void SetCurrentPlayerTurn(int playerIndex)
    {
        if (!isBiddingActive) return;
        
        // Disable all players first
        foreach (var input in playerInputs)
        {
            input.SetIsMyTurn(false);
        }
        
        // Enable the current player
        if (playerInputMap.TryGetValue(playerIndex, out var currentPlayerInput))
        {
            currentPlayerInput.SetIsMyTurn(true);
            currentTurnPlayerIndex = playerIndex;
            
            Debug.Log($"TurnManager: Player {playerIndex}'s turn");
            
            // Notify BidChooser if it exists
            if (bidChooser != null)
            {
                // Find the index in the players list
                int listIndex = players.FindIndex(p => p.PlayerIndex == playerIndex);
                if (listIndex >= 0)
                {
                    // This will trigger BidChooser to move the panel
                    bidChooser.MovePanelToPlayer(listIndex); // Now this is public!
                }
            }
        }
        else
        {
            Debug.LogWarning($"TurnManager: Player {playerIndex} not found in input map");
        }
    }
    
    public void MoveToNextPlayer()
    {
        if (!isTurnBased || !isBiddingActive) return;
        
        // Find next valid player index
        int nextIndex = (currentTurnPlayerIndex + 1) % (players.Count + 1);
        if (nextIndex >= players.Count) nextIndex = 0;
        
        // Check if we should skip this player (based on game rules)
        // For now, just move to next
        SetCurrentPlayerTurn(nextIndex);
    }
    
    public void OnPlayerBidPlaced(int playerIndex)
    {
        Debug.Log($"TurnManager: Player {playerIndex} placed bid");
        
        if (isTurnBased)
        {
            // Move to next player after a short delay
            Invoke(nameof(DelayedNextPlayer), 0.3f);
        }
    }
    
    private void DelayedNextPlayer()
    {
        MoveToNextPlayer();
    }
    
    public void EnableAllPlayers()
    {
        Debug.Log("TurnManager: Enabling all players (free-for-all)");
        
        isTurnBased = false;
        isBiddingActive = true;
        
        foreach (var input in playerInputs)
        {
            input.SetIsMyTurn(true);
        }
    }
    
    public void EnableTurnBased()
    {
        Debug.Log("TurnManager: Enabling turn-based mode");
        
        isTurnBased = true;
        SetCurrentPlayerTurn(currentTurnPlayerIndex);
    }
    
    public void EndBiddingPhase()
    {
        Debug.Log("TurnManager: Ending bidding phase");
        
        isBiddingActive = false;
        
        foreach (var input in playerInputs)
        {
            input.SetIsMyTurn(false);
            input.SetBiddingPhaseActive(false);
        }
    }
    
    public PlayerBiddingInput GetPlayerInput(int playerIndex)
    {
        playerInputMap.TryGetValue(playerIndex, out var input);
        return input;
    }
    
    void OnDestroy()
    {
        // Clean up any invokes
        CancelInvoke();
    }
}