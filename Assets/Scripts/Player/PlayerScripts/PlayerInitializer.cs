using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInitializer : MonoBehaviour
{
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject playerHUDPrefab;
    [SerializeField] HorizontalLayoutGroup layoutGroup;
    [SerializeField] CinemachineTargetGroup targetGroup;

    private void Start()
    {
        var configs = PlayerConfigManager.Instance.GetPlayerConfigs().ToArray();

        for (int i = 0; i < configs.Length; i++)
        {
            var player = Instantiate(playerPrefab, spawnPoints[i].position, spawnPoints[i].rotation, transform);
            var playerHUD = Instantiate(playerHUDPrefab, layoutGroup.transform);

            targetGroup.Targets[i].Object = player.transform;
            var inp = player.GetComponent<PlayerInputHandler>();
            inp.InitializePlayer(configs[i]);

            playerHUD.GetComponent<PlayerHUD>().InitHUD(configs[i], inp.ctx);

        }

    }


}
