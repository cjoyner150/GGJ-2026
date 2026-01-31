using System.Collections.Generic;
using UnityEngine;

public class BiddingSession : MonoBehaviour
{
    public event System.Action<PlayerGold> OnMaskWinner;

    private List<PlayerGold> playerGolds;
    private HashSet<PlayerGold> bidders = new();

    public BiddingSession(List<PlayerGold> playerGoldArray)
    {
        this.playerGolds = playerGoldArray;
    }
    public void TryBid(PlayerGold bidder, int amount)
    {
        if (!bidder.HasEnoughGold(amount)) return;
        if (bidders.Contains(bidder)) return;

        bidder.SpendGold(amount);
        bidders.Add(bidder);
        if (bidders.Count == playerGolds.Count - 1)
        {
            foreach (var player in playerGolds)
            {
                if (!bidders.Contains(player))
                {
                    OnMaskWinner?.Invoke(player);
                    break;
                }
            }
        }

    }

}
