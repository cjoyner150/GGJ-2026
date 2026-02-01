using System;
using System.Collections.Generic;

public class BiddingSession : BaseBiddingSession
{
    public event Action<PlayerGold, bool> OnMaskAssigned; // player, isBlessed

    private readonly HashSet<PlayerGold> bidders = new();

    public BiddingSession(List<PlayerGold> players, int baseBidAmount) 
        : base(players, baseBidAmount) { }

    public override bool TryBid(PlayerGold player, int bidAmount)
    {
        if (!base.TryBid(player, bidAmount)) return false;

        bidders.Add(player);
        UnityEngine.Debug.Log($"Bidding: {GetBidderCount()}/{GetEligiblePlayerCount()} players bid");
        return true;
    }

    public void ForceLowestBidderToTakeMask()
    {
        // Find player who hasn't bid yet
        PlayerGold nonBidder = players.Find(p => !excludedPlayers.Contains(p) && !bidders.Contains(p));
        if (nonBidder != null)
        {
            AssignMask(nonBidder, false);
            UnityEngine.Debug.Log($"Timer expired! Player {nonBidder.PlayerIndex} forced to take mask");
            return;
        }

        // All bid - find first eligible player
        PlayerGold lowestBidder = players.Find(p => !excludedPlayers.Contains(p));
        if (lowestBidder != null)
        {
            AssignMask(lowestBidder, false);
            UnityEngine.Debug.Log($"Timer expired! Player {lowestBidder.PlayerIndex} takes mask");
        }
    }

    private void AssignMask(PlayerGold player, bool isBlessed)
    {
        excludedPlayers.Add(player);
        OnMaskAssigned?.Invoke(player, isBlessed);
        CheckForBlessedMask();
    }

    private void CheckForBlessedMask()
    {
        // If only one player left without a mask, they get blessed mask
        var remaining = players.FindAll(p => !excludedPlayers.Contains(p));
        if (remaining.Count == 1)
        {
            PlayerGold lastPlayer = remaining[0];
            excludedPlayers.Add(lastPlayer);
            OnMaskAssigned?.Invoke(lastPlayer, true); // BLESSED!
            UnityEngine.Debug.Log($"Player {lastPlayer.PlayerIndex} gets BLESSED MASK!");
        }
    }

    public void ResetForNewMask()
    {
        bidders.Clear();
        currentBidAmount = baseBidAmount;
        // excludedPlayers persists across masks
    }

    public override void Reset()
    {
        base.Reset();
        bidders.Clear();
    }

    public List<PlayerGold> GetPlayersWhoHaventBid() => 
        players.FindAll(p => !excludedPlayers.Contains(p) && !bidders.Contains(p));

    private int GetBidderCount() => players.FindAll(p => !excludedPlayers.Contains(p) && bidders.Contains(p)).Count;
    private int GetEligiblePlayerCount() => players.FindAll(p => !excludedPlayers.Contains(p)).Count;
}