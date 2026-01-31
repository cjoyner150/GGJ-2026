using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInitializer : MonoBehaviour
{
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] GameObject playerPrefab;

    private void Start()
    {
        var configs = PlayerConfigManager.Instance.GetPlayerConfigs().ToArray();

        for (int i = 0; i < configs.Length; i++)
        {
            var player = Instantiate(playerPrefab, spawnPoints[i].position, spawnPoints[i].rotation, transform);

            player.GetComponent<PlayerInputHandler>().InitializePlayer(configs[i]);
        }
    }


}
