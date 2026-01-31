using System.Collections.Generic;
using UnityEngine;

public class MaskDeck
{
    private List<Mask> deck;
    private int index;

    public MaskDeck(List<Mask> masks)
    {
        deck = new List<Mask>(masks);
        Shuffle();
    }

    public Mask Draw()
    {
        return deck[index++];
    }

    private void Shuffle()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int r = Random.Range(i, deck.Count);
            (deck[i], deck[r]) = (deck[r], deck[i]);
        }
    }
}
