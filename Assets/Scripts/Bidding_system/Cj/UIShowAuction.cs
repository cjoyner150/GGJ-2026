using TMPro;
using UnityEngine;

public class UIShowAuction : MonoBehaviour
{
    [Header("Mask")]
    [SerializeField] TextMeshProUGUI nameTMP;
    [SerializeField] TextMeshProUGUI descriptionTMP;
    [SerializeField] TextMeshProUGUI maskDoomTMP;

    [Header("Cam")]
    [SerializeField] Transform spawnLocation;

    GameObject currentMaskObj;

    public void UpdateMask(MaskObject mask)
    {
        if (currentMaskObj != null) Destroy(currentMaskObj);

        currentMaskObj = Instantiate(mask.maskPrefab, spawnLocation);
        currentMaskObj.layer = 7;
        nameTMP.text = mask.name;
        descriptionTMP.text = mask.maskDescription;
        maskDoomTMP.text = $"DOOM: {mask.maskDoom}";
    }

    public void UpdateTarot(TarotObject tarot)
    {

    }
}
