using System.Collections;
using UnityEngine;

public class BiddingSceneStarter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BiddingSystemInitializer biddingInitializer;
    
    [Header("Settings")]
    [SerializeField] private bool autoStartBidding = true;
    [SerializeField] private float startDelay = 1f;
    
    void Start()
    {
        if (autoStartBidding)
        {
            StartCoroutine(StartBiddingDelayed());
        }
    }
    
    IEnumerator StartBiddingDelayed()
    {
        yield return new WaitForSeconds(startDelay);
        
        // Initialize the bidding system
        if (biddingInitializer == null)
            biddingInitializer = FindObjectOfType<BiddingSystemInitializer>();
            
        if (biddingInitializer != null)
        {
            biddingInitializer.Initialize();
            
            // Start mask phase after a short delay
            StartCoroutine(StartMaskPhaseDelayed(0.5f));
        }
        else
        {
            Debug.LogError("No BiddingSystemInitializer found in bidding scene!");
        }
    }
    
    IEnumerator StartMaskPhaseDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (biddingInitializer != null)
        {
            biddingInitializer.StartMaskPhase();
        }
    }
    
    // Manual start from UI button
    public void StartBidding()
    {
        if (biddingInitializer != null)
        {
            biddingInitializer.Initialize();
            biddingInitializer.StartMaskPhase();
        }
    }
}