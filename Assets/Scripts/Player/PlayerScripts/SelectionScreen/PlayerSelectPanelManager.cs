using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerSelectPanelManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI playerTitleTMP;
    [SerializeField] GameObject readyVisual;
    [SerializeField] Image image;
    [SerializeField] RenderTexture[] renderTextures;
    [SerializeField] Camera cam;
    [SerializeField] RawImage rawIMG;
    [SerializeField] SkinnedMeshRenderer playerMesh;


    private int playerIndex;
    private bool inputEnabled = false;

    private void Awake()
    {
        readyVisual.SetActive(false);
    }

    public void SetPlayer (PlayerInput inp)
    {
        playerIndex = inp.playerIndex;
        playerTitleTMP.text = "Player " + (playerIndex + 1);

        Color col;
        switch (playerIndex)
        {
            case 0:
                col = new Color(107f/255f, 128f / 255f, 103f / 255f);
                break;
            case 1:
                col = new Color(179f / 255f, 55f / 255f, 44f / 255f);
                break;
            case 2:
                col = new Color(115f / 255f, 67f / 255f, 102f / 255f);
                break;
            case 3:
                col = new Color(244f / 255f, 150f / 255f, 68f / 255f);
                break;
            default:
                col = Color.red;
                break;
        }

        cam.targetTexture = renderTextures[playerIndex];
        rawIMG.texture = cam.targetTexture;

        PlayerConfigManager.Instance.SetPlayerColor(playerIndex, col);
        playerMesh.material.color = col;
        image.color = col;

        Invoke(nameof(EnableInput), .3f);
    }

    private void EnableInput()
    {
        inputEnabled = true;
    }

    //public void SetSelection()
    //{
    //    PlayerConfigManager.Instance.SetPlayerMesh(playerIndex, selection.currentSelection.meshPrefab, selection.currentSelection.Name);
    //}

    public void SetReady()
    {
        if (!inputEnabled) return;

        PlayerConfigManager.Instance.SetPlayerReady(playerIndex);
        readyVisual.SetActive(true);
    }

    
}
