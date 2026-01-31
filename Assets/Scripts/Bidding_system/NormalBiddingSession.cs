using System;
using System.Collections.Generic;

public class NormalBiddingSession
{
    public event Action<PlayerGold> OnBidWinner;

    private PlayerGold highestBidder;
    private int highestBid;
    private readonly int bidIncrement;

    public NormalBiddingSession(int bidIncrement)
    {
        this.bidIncrement = bidIncrement;
    }

    public void TryBid(PlayerGold player)
    {
        if (!player.CanAfford(bidIncrement)) return;

        if (player.TrySpend(bidIncrement))
        {
            highestBid += bidIncrement;
            highestBidder = player;
        }
    }

    public void EndBidding()
    {
        if (highestBidder != null)
            OnBidWinner?.Invoke(highestBidder);
    }
}