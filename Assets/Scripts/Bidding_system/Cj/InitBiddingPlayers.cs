using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class InitBiddingPlayers : MonoBehaviour
{
    [SerializeField] GameObject biddingPlayerPrefab;
    [SerializeField] HorizontalLayoutGroup hlg;
    [SerializeField] BiddingManager biddingManager;

    void Awake()
    {
        var configs = PlayerConfigManager.Instance.GetPlayerConfigs();

        List<BiddingInputHandler> players = new List<BiddingInputHandler>();

        for (int i = 0; i<configs.Count; i++)
        {
            var player = Instantiate(biddingPlayerPrefab, hlg.transform);
            configs[i].Input.uiInputModule = player.GetComponentInChildren<InputSystemUIInputModule>();

            var inp = player.GetComponent<BiddingInputHandler>();
            inp.Initialize(configs[i]);
            players.Add(inp);
        }

        biddingManager.Initialize(players);
    }
}
