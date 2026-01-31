using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class MaskBiddingSession : MonoBehaviour
{
    private PlayerDesitionTimer timer;

    PlayerTurnManager turnManager;
    BiddingSession biddingSession;

    public void startMaskBidding(List<PlayerGold> Players)
    {
        turnManager = new PlayerTurnManager(Players);
        biddingSession = new BiddingSession(Players);
        
        biddingSession.OnMaskWinner += winner => AssignMask(winner);
        timer.OnTimeOut += onMaskBidTimeOut;
        StartTurn();

    }
    void StartTurn()
    {
        timer.StartTimer();
        
    }
    public void playerBid(PlayerGold player, int amount)
    {
        biddingSession.TryBid(player, amount);
        if (turnManager.isOnlyOnePlayerLeft())
        {
            return;
        }
        turnManager.nextPlayerTurn();
        StartTurn();
    }
    void onMaskBidTimeOut()
    {
        //biddingSession.ForceEnd();
    }
    public void AssignMask(PlayerGold winner)
    {
        timer.StopTimer();
    }
    public void HilightPlayer(PlayerGold player)
    {
        // Highlight player UI
    }
}
