using UnityEngine;

[System.Serializable]
public class TarotObject
{
    public string name;
    public Effector tarotEffector;
    public GameObject tarotPrefab;
    public string description;
    public Sprite UIAsset;

    public TarotObject(TarotSO tarotSO)
    {
        name = tarotSO.tarotName;
        tarotPrefab = tarotSO.tarotPrefab;
        description = tarotSO.description;
        UIAsset = tarotSO.UIAsset;
    }
}
