using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] IntEventSO playerDiedEvent;
    [SerializeField] IntEventSO roundWonEvent;

    List<PlayerConfig> configs;

    private void OnEnable()
    {
        playerDiedEvent.onEventRaised += OnPlayerDied;
        roundWonEvent.onEventRaised += OnRoundWin;
    }

    private void OnDisable()
    {
        playerDiedEvent.onEventRaised -= OnPlayerDied;
        roundWonEvent.onEventRaised -= OnRoundWin;
    }

    void Start()
    {
        configs = PlayerConfigManager.Instance.GetPlayerConfigs();
    }

    void OnPlayerDied(int playerIndex)
    {
        configs.Remove(configs.Find(c => c.PlayerIndex == playerIndex));

        if (configs.Count == 1)
        {
            roundWonEvent.onEventRaised.Invoke(configs[0].PlayerIndex);
        }
    }

    void OnRoundWin(int playerIndex)
    {
        print($"Player won: {playerIndex + 1}");
    }

}
