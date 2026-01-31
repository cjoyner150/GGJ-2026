using UnityEngine;

public class BiddingSession : MonoBehaviour
{
    public event System.Action OnBiddingStarted;
    public event System.Action OnBiddingFinished;



    public void StartBidding()
    {
        Debug.Log("Bidding session started.");
        OnBiddingStarted?.Invoke();
    }
    public void FinishBidding()
    {
        Debug.Log("Bidding session finished.");
        OnBiddingFinished?.Invoke();
    }

}
