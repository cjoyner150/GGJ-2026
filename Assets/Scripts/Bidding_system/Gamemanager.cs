using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class Gamemanager : MonoBehaviour
{
    [SerializeField] MaskBiddingController biddingController;
    [SerializeField] BidChooser bidChooser;
    
    void Start()
    {
        StartCoroutine(StartGameSequence());
    }
    
    IEnumerator StartGameSequence()
    {
        yield return new WaitForSeconds(1f); // Wait for everything to initialize
        
        // Show "Game starting..." message
        Debug.Log("Starting mask phase...");
        
        // Start mask phase
        if (biddingController != null)
        {
            biddingController.BeginMaskPhase();
        }
    }
}