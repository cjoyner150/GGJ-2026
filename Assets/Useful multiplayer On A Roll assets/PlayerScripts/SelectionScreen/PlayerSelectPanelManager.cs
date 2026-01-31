using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSelectPanelManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI playerTitleTMP;
    [SerializeField] GameObject readyVisual;

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

        Invoke(nameof(EnableInput), 1.5f);
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
