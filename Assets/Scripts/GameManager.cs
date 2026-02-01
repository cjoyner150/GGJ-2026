using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private VoidEventSO roundEndedEvent;

    [Header("Scene Indexes")]
    [SerializeField] private int startSceneIndex = 0;
    [SerializeField] private int biddingSceneIndex = 1;
    [SerializeField] private int fightSceneIndex = 2;

    public static GameManager Instance;
    private bool x = false;
    public PlayerConfigManager PlayerConfigManager => PlayerConfigManager.Instance;

    public bool RoundActive { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        if (roundEndedEvent != null)
            roundEndedEvent.onEventRaised += RegisterRoundWinner;
    }

    private void OnDisable()
    {
        if (roundEndedEvent != null)
            roundEndedEvent.onEventRaised -= OnRoundEnded;
    }

    private void OnRoundEnded()
    {
        Debug.Log("GameManager received Round Ended event!");

        var winner = PlayerConfigManager.GetPlayerConfigs()[0];

        EndRound(winner);
    }

    public void StartRound()
    {
        RoundActive = true;
        Debug.Log("Round Started");
    }

    public void EndRound(PlayerConfig winner)
    {
        if (!RoundActive) return;

        RoundActive = false;

        Debug.Log("Round Ended");

        if (roundEndedEvent != null)
            roundEndedEvent.RaiseEvent();

        // Automatically register winner
        RegisterRoundWinner();
    }

    // Call this when a player wins a fight
    public void RegisterRoundWinner()
    {
        var players = PlayerConfigManager.GetPlayerConfigs();

        LoadBiddingScene();

    }

    private void LoadBiddingScene()
    {
        Debug.Log("Loading Bidding Scene...");

        ClearPlayerRoundData();

        SceneManager.LoadScene(biddingSceneIndex);
    }

    private void EndGame()
    {
        Debug.Log("A player reached 2 wins – returning to Start Scene");

        ResetAllPlayersCompletely();

        SceneManager.LoadScene(startSceneIndex);
    }

    // Clear temporary data between rounds (acorns, masks, tarots)
    private void ClearPlayerRoundData()
    {
        var players = PlayerConfigManager.GetPlayerConfigs();

        foreach (var player in players)
        {
            player.Acorns = 0;

            player.Mask = null;
            player.Tarots.Clear();
        }

        Debug.Log("Player round data cleared for next bidding phase");
    }

    private void ResetAllPlayersCompletely()
    {
        var players = PlayerConfigManager.GetPlayerConfigs();

        foreach (var player in players)
        {
            player.RoundsWon = 0;
            player.Acorns = 0;

            player.Mask = null;
            player.Tarots.Clear();
            player.IsReady = false;
        }

        Debug.Log("All player progress reset – returning to start scene");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game");
        Application.Quit();
    }
}