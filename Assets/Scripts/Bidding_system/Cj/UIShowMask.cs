using TMPro;
using UnityEngine;

public class UIShowMask : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameTMP;
    [SerializeField] TextMeshProUGUI descriptionTMP;
    [SerializeField] TextMeshProUGUI maskDoomTMP;
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
}
