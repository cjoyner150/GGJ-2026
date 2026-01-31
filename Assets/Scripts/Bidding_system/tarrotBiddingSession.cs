using UnityEngine;
using System.Collections.Generic;
using System;

public class tarrotBiddingSession : MonoBehaviour
{
    public event Action<PlayerGold> Onwinner;
    public event Action<PlayerGold> Onplayerkicked;

    private Dictionary<PlayerGold, int> Bids;
    private HashSet<PlayerGold> Players;


    public void playerBid(PlayerGold player, int amount)
    {
        if (!Players.Contains(player)) return;
        if (!player.HasEnoughGold(amount)) return;
        player.SpendGold(amount);
        if (!Bids.ContainsKey(player))
        {
            Bids[player] = 0;
        }
        Bids[player] += amount;
    }
    public void kickPlayer(PlayerGold player)
    {
        if (!Players.Contains(player)) return;
        Players.Remove(player);
        Onplayerkicked?.Invoke(player);
        if (Players.Count == 1)
        {
            foreach (var p in Players)
            {
                Onwinner?.Invoke(p);
                break;
            }
        }
    }
    public void forceEndBidding()
    {
        endBidding();
    }
    public void endBidding()
    {
        PlayerGold highestBidder = null;
        int highestBid = -1;
        foreach (var bid in Bids)
        {
            if (bid.Value > highestBid)
            {
                highestBid = bid.Value;
                highestBidder = bid.Key;
            }
        }
        if (highestBidder != null)
        {
            Onwinner?.Invoke(highestBidder);
        }
    }
}
