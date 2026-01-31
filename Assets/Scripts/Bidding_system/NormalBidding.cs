using UnityEngine;

public class NormalBidding : MonoBehaviour
{
    event System.Action OnBidWinner;

    private PlayerGold Highestbid;
    private int HighestBidAmount;

    private int bidIncrement;
    public NormalBidding(int bidIncrement)
    {
        this.bidIncrement = bidIncrement;
    }
    public void Trybid(PlayerGold player) 
    { 
        int newBidAmount = HighestBidAmount + bidIncrement;
        if (player.HasEnoughGold(newBidAmount))
        {
            player.SpendGold(newBidAmount);
            Highestbid = player;
            HighestBidAmount = newBidAmount;
        }
    }
    public void EndBidding()
    {
        OnBidWinner?.Invoke();
    }

}
