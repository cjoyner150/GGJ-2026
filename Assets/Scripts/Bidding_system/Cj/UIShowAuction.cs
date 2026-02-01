using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIShowAuction : MonoBehaviour
{
    [Header("Mask")]
    [SerializeField] TextMeshProUGUI nameTMP;
    [SerializeField] TextMeshProUGUI descriptionTMP;
    [SerializeField] TextMeshProUGUI maskDoomTMP;

    [Header("Cam")]
    [SerializeField] Transform spawnLocation;

    GameObject currentObj;

    public void UpdateMask(MaskObject mask)
    {
        if (currentObj != null) Destroy(currentObj);

        currentObj = Instantiate(mask.maskPrefab, spawnLocation);
        currentObj.layer = 7;
        nameTMP.text = mask.name;
        descriptionTMP.text = mask.maskDescription;
        maskDoomTMP.text = $"DOOM: {mask.maskDoom}";
    }

    public void UpdateTarot(TarotObject tarot)
    {
        spawnLocation.rotation = Quaternion.identity;

        if (currentObj != null) Destroy(currentObj);

        currentObj = Instantiate(tarot.tarotPrefab, spawnLocation);
        currentObj.layer = 7;
        nameTMP.text = tarot.name;
        descriptionTMP.text = tarot.description;
        maskDoomTMP.text = "";
    }
}
