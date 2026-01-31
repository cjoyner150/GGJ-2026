using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class SpawnPlayerSelection : MonoBehaviour
{
    [SerializeField] GameObject selectionElementPrefab;
    [SerializeField] PlayerInput inp;
    HorizontalLayoutGroup group;

    private void Awake()
    {
        group = FindAnyObjectByType<HorizontalLayoutGroup>();

        var menu = Instantiate(selectionElementPrefab, group.transform);
        inp.uiInputModule = menu.GetComponentInChildren<InputSystemUIInputModule>();

        menu.GetComponentInChildren<PlayerSelectPanelManager>().SetPlayer(inp);

    }
}
