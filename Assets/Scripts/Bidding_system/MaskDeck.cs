using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class MaskDeck : MonoBehaviour
{
    public List<Masks> masks;
    public int Index = 0;

    public MaskDeck(List<Masks> masks)
    {
        masks = new List<Masks>();
        shuffleDeck();
    }
    public Masks drawMask()
    {
        if (Index >= masks.Count)
        {
            Index = 0;
            shuffleDeck();
        }
        Masks drawnMask = masks[Index];
        Index++;
        return drawnMask;
    }
    
    public void shuffleDeck()
    {
        System.Random rng = new System.Random();
        int n = masks.Count;
        while (n > 1)
        {
            int k = rng.Next(n--);
            Masks temp = masks[n];
            masks[n] = masks[k];
            masks[k] = temp;
        }
    }
}
