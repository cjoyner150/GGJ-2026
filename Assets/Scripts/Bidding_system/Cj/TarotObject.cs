using UnityEngine;

[System.Serializable]
public class TarotObject
{
    public enum cardType
    {
        Chariot,
        Empress,
        HighPriestess, 
        Star, 
        Moon,
        Devil,
        Magician,
        Wheel
    }

    public string name;
    public Effector tarotEffector;
    public GameObject tarotPrefab;
    public string description;
    public Sprite UIAsset;
    public cardType type;

    public TarotObject(TarotSO tarotSO)
    {
        name = tarotSO.tarotName;
        tarotPrefab = tarotSO.tarotPrefab;
        description = tarotSO.description;
        UIAsset = tarotSO.UIAsset;
        type = tarotSO.type;
    }
}
