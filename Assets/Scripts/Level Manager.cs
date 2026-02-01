using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] IntEventSO playerDiedEvent;
    [SerializeField] IntEventSO roundWonEvent;

    [SerializeField] GameObject roundWinTimeline;
    [SerializeField] TextMeshProUGUI winTMP;

    [SerializeField] GameObject gameplayCanvas;
    [SerializeField] SkinnedMeshRenderer playerWinRenderer;

    public MMF_Player fader;

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
        configs = PlayerConfigManager.Instance.GetPlayerConfigs().ToArray().ToList();
        fader?.PlayFeedbacks();
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
        PlayerConfig cfg = PlayerConfigManager.Instance.GetPlayerConfigs()[playerIndex];

        cfg.RoundsWon++;

        AudioManager.Instance?.VoiceEndDelayed("Bee", 1.5f);

        winTMP.text = $"Player {playerIndex + 1} won the round! " +
            $"\nPlayer {playerIndex + 1} has {cfg.RoundsWon} wins.";
        roundWinTimeline.SetActive(true);
        gameplayCanvas.SetActive(false);

        playerWinRenderer.material.color = cfg.PlayerColor;

    }



}
