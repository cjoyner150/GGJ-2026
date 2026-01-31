using System;
using System.Collections.Generic;

public class BiddingSession
{
    public event Action<PlayerGold> OnMaskWinner;

    private readonly List<PlayerGold> players;
    private readonly HashSet<PlayerGold> bidders = new();
    private readonly int bidCost;

    public BiddingSession(List<PlayerGold> players, int bidCost)
    {
        this.players = players;
        this.bidCost = bidCost;
    }

    public void TryBid(PlayerGold player)
    {
        if (!player.CanAfford(bidCost)) return;
        if (bidders.Contains(player)) return;

        if (player.TrySpend(bidCost))
        {
            bidders.Add(player);

            if (bidders.Count == players.Count - 1)
            {
                foreach (var p in players)
                {
                    if (!bidders.Contains(p))
                    {
                        OnMaskWinner?.Invoke(p);
                        break;
                    }
                }
            }
        }
    }
}