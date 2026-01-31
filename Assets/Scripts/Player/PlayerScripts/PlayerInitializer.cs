using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerInitializer : MonoBehaviour
{
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] CinemachineTargetGroup targetGroup;

    private void Start()
    {
        var configs = PlayerConfigManager.Instance.GetPlayerConfigs().ToArray();

        for (int i = 0; i < configs.Length; i++)
        {
            var player = Instantiate(playerPrefab, spawnPoints[i].position, spawnPoints[i].rotation, transform);

            targetGroup.Targets[i].Object = player.transform;
            player.GetComponent<PlayerInputHandler>().InitializePlayer(configs[i]);
        }
    }


}
