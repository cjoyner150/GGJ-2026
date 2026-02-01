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
                col = new Color(91, 128, 103);
                break;
            case 1: 
                col = new Color(179, 55, 44);
                break;
            case 2: 
                col = new Color(115, 67, 102);
                break;
            case 3:
                col = new Color(244, 150, 68);
                break;
            default:
                col = Color.white;
                break;
        }

        PlayerConfigManager.Instance.SetPlayerColor(playerIndex, col);
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
