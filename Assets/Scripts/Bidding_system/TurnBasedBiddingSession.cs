using System;
using System.Collections.Generic;
using UnityEngine;

public class TurnBasedBiddingSession
{
    public event Action<int> OnPotUpdated;
    public event Action<int> OnIncrementChanged;
    public event Action<PlayerGold> OnTurnChanged;
    public event Action<PlayerGold, int> OnPlayerBid;
    
    private List<PlayerGold> players;
    private int currentPlayerIndex;
    private int currentIncrement;
    private int pot;
    private float turnTimer;
    private float turnDuration = 5f;
    
    private PlayerGold currentHighestBidder;
    private int currentHighestBid;
    
    public int CurrentIncrement => currentIncrement;
    public int Pot => pot;
    public PlayerGold CurrentPlayer => players[currentPlayerIndex];
    public float TimeRemaining => turnTimer;
    
    public TurnBasedBiddingSession(List<PlayerGold> players, int startingIncrement)
    {
        this.players = players;
        currentIncrement = startingIncrement;
        currentPlayerIndex = 0;
        pot = 0;
        turnTimer = turnDuration;
    }
    
    public void StartTurn()
    {
        turnTimer = turnDuration;
        OnTurnChanged?.Invoke(CurrentPlayer);
    }
    
    public void Update(float deltaTime)
    {
        if (turnTimer > 0)
        {
            turnTimer -= deltaTime;
            if (turnTimer <= 0)
            {
                AutoSubmitBid();
            }
        }
    }
    
    public bool TryPlaceBid(PlayerGold player, int bidAmount)
    {
        if (player != CurrentPlayer) return false;
        if (bidAmount < currentIncrement) return false;
        
        if (player.TrySpend(bidAmount))
        {
            pot += bidAmount;
            
            // Update increment if bid is higher
            if (bidAmount > currentIncrement)
            {
                currentIncrement = bidAmount;
                OnIncrementChanged?.Invoke(currentIncrement);
            }
            
            currentHighestBid = bidAmount;
            currentHighestBidder = player;
            
            OnPlayerBid?.Invoke(player, bidAmount);
            OnPotUpdated?.Invoke(pot);
            
            NextTurn();
            return true;
        }
        
        return false;
    }
    
    private void AutoSubmitBid()
    {
        TryPlaceBid(CurrentPlayer, currentIncrement);
    }
    
    private void NextTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        StartTurn();
    }
    
    public void EndSession()
    {
        // Distribute pot or handle end logic
    }
}