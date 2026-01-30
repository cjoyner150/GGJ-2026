using UnityEngine;
using System.Collections;

public class PlayerGold : MonoBehaviour
{
    public int goldAmount;

    public bool SpendGold(int amount)
    {
        if (amount > goldAmount)
        {
            return false; // Not enough gold
        }
        goldAmount -= amount;
        return true; // Successfully spent gold
    }

    public void AddGold(int amount)
    {
        goldAmount += amount;
    }

    public void SetGold(int amount)
    {
        goldAmount = amount;
    }
    
}
