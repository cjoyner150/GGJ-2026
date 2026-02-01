using System;
using System.Collections.Generic;

public abstract class BaseBiddingSession
{
    public event Action<int> OnCurrentBidAmountChanged;
    
    protected readonly List<PlayerGold> players;
    protected readonly HashSet<PlayerGold> excludedPlayers = new();
    protected int currentBidAmount;
    protected readonly int baseBidAmount;

    public int CurrentBidAmount => currentBidAmount;

    protected BaseBiddingSession(List<PlayerGold> players, int baseBidAmount)
    {
        this.players = players;
        this.baseBidAmount = baseBidAmount;
        this.currentBidAmount = baseBidAmount;
    }

    public virtual bool TryBid(PlayerGold player, int bidAmount)
    {
        if (excludedPlayers.Contains(player))
        {
            UnityEngine.Debug.Log($"Player {player.PlayerIndex} is excluded from bidding");
            return false;
        }

        if (bidAmount < currentBidAmount)
        {
            UnityEngine.Debug.Log($"Player {player.PlayerIndex} bid {bidAmount} but minimum is {currentBidAmount}");
            return false;
        }

        if (!player.CanAfford(bidAmount) || !player.TrySpend(bidAmount))
        {
            UnityEngine.Debug.Log($"Player {player.PlayerIndex} cannot afford {bidAmount}");
            return false;
        }

        if (bidAmount > currentBidAmount)
        {
            currentBidAmount = bidAmount;
            OnCurrentBidAmountChanged?.Invoke(currentBidAmount);
            UnityEngine.Debug.Log($"Player {player.PlayerIndex} raised to {currentBidAmount}");
        }

        return true;
    }

    public virtual void Reset()
    {
        excludedPlayers.Clear();
        currentBidAmount = baseBidAmount;
    }

    public bool CanPlayerBid(PlayerGold player) => !excludedPlayers.Contains(player);
}