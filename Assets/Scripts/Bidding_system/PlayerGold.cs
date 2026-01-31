using UnityEngine;
using System.Collections;

public class PlayerGold : MonoBehaviour
{
    public int goldAmount;

    public bool HasEnoughGold(int amount)
    {
        return goldAmount >= amount;
    }
    public void SpendGold(int amount)
    {
        goldAmount -= amount;
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
