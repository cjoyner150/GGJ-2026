using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class NormalBiddingSession
{
    public event Action<PlayerGold> OnBidWinner;
    public event Action<int> OnCurrentBidAmountChanged;
    
    private List<PlayerGold> players;
    private HashSet<PlayerGold> passedPlayers = new HashSet<PlayerGold>();
    private HashSet<PlayerGold> wonTarotPlayers = new HashSet<PlayerGold>();
    private Dictionary<PlayerGold, int> playerBids = new Dictionary<PlayerGold, int>();
    private int currentBidAmount;
    private int baseBidIncrement;
    private bool biddingEnded = false; // FIX: Track if bidding has ended
    
    public int CurrentBidAmount => currentBidAmount;
    
    public NormalBiddingSession(List<PlayerGold> players, int baseBidIncrement)
    {
        this.players = players;
        this.baseBidIncrement = baseBidIncrement;
        currentBidAmount = baseBidIncrement;
    }
    
    public void ResetForNewTarot()
    {
        passedPlayers.Clear();
        playerBids.Clear();
        currentBidAmount = baseBidIncrement;
        biddingEnded = false; // FIX: Reset for new tarot
        OnCurrentBidAmountChanged?.Invoke(currentBidAmount);
    }
    
    public bool TryBid(PlayerGold player, int bidAmount)
    {
        if (player == null || bidAmount < currentBidAmount || biddingEnded) 
            return false;
        
        playerBids[player] = bidAmount;
        currentBidAmount = bidAmount;
        OnCurrentBidAmountChanged?.Invoke(currentBidAmount);
        return true;
    }
    
    public void PlayerPasses(PlayerGold player)
    {
        if (player == null || biddingEnded) return;
        
        passedPlayers.Add(player);
        Debug.Log($"Player {player.PlayerIndex} passed");
        
        CheckForAutomaticWin();
    }
    public bool HasActiveBidding()
    {
        int activePlayers = 0;
        
        foreach (var player in players)
        {
            if (!passedPlayers.Contains(player) && !wonTarotPlayers.Contains(player))
            {
                activePlayers++;
            }
        }
        
        return activePlayers > 1;
    }
    private void CheckForAutomaticWin()
    {
        // Count players who can still bid (haven't passed and haven't won)
        int activePlayers = 0;
        PlayerGold lastActivePlayer = null;
        
        foreach (var player in players)
        {
            if (!passedPlayers.Contains(player) && !wonTarotPlayers.Contains(player))
            {
                activePlayers++;
                lastActivePlayer = player;
            }
        }
        
        Debug.Log($"{activePlayers} players can still bid");
        
        // If only one player left, they win automatically
        if (activePlayers == 1 && lastActivePlayer != null)
        {
            Debug.Log($"Only one player left - {lastActivePlayer.PlayerIndex} wins automatically");
            EndBidding();
        }
        else if (activePlayers == 0)
        {
            Debug.Log("No players left - ending bidding");
            EndBidding();
        }
    }
    
    public void EndBidding()
    {
        // FIX: Prevent duplicate ending
        if (biddingEnded)
        {
            Debug.LogWarning("Bidding already ended - ignoring duplicate EndBidding call");
            return;
        }
        
        biddingEnded = true;
        
        // Find highest bidder
        PlayerGold highestBidder = null;
        int highestBid = 0;
        
        foreach (var kvp in playerBids)
        {
            if (kvp.Value > highestBid)
            {
                highestBid = kvp.Value;
                highestBidder = kvp.Key;
            }
        }
        
        // If no bids, find first player who didn't pass
        if (highestBidder == null)
        {
            foreach (var player in players)
            {
                if (!passedPlayers.Contains(player))
                {
                    highestBidder = player;
                    break;
                }
            }
        }
        
        if (highestBidder != null)
        {
            wonTarotPlayers.Add(highestBidder);
            Debug.Log($"Player {highestBidder.PlayerIndex} wins tarot");
            OnBidWinner?.Invoke(highestBidder);
        }
        else
        {
            Debug.LogError("No winner found for tarot!");
        }
    }
    
    public bool HasPlayerWonTarot(PlayerGold player) => 
        player != null && wonTarotPlayers.Contains(player);
}
