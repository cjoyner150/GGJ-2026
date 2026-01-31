// Save this as SetupBiddingData.cs
using UnityEngine;
using System.Collections.Generic;

public class SetupBiddingData : MonoBehaviour
{
    public MaskBiddingController biddingController;
    
    void Start()
    {
        if (biddingController == null)
            biddingController = FindObjectOfType<MaskBiddingController>();
        
        SetupTestData();
    }
    
    void SetupTestData()
    {
        // Create test masks
        biddingController.masks = new List<Mask>
        {
            new Mask { type = MaskType.Cursed, doomLevel = 1, description = "Cursed Mask of Shadows" },
            new Mask { type = MaskType.Clean, doomLevel = 0, description = "Clean Mask of Light" },
            new Mask { type = MaskType.Cursed, doomLevel = 2, description = "Mask of Eternal Night" },
            new Mask { type = MaskType.Clean, doomLevel = 0, description = "Mask of Dawn" }
        };
        
        // Create test tarot cards
        biddingController.tarotCards = new List<TarotCard>
        {
            new TarotCard { name = "The Fool", description = "New beginnings" },
            new TarotCard { name = "The Magician", description = "Manifestation" },
            new TarotCard { name = "The High Priestess", description = "Intuition" },
            new TarotCard { name = "The Emperor", description = "Authority" }
        };
        
        Debug.Log("Test data setup complete!");
    }
}