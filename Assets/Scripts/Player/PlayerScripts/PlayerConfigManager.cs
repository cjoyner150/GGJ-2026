using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class PlayerConfigManager : MonoBehaviour
{
    [SerializeField] private int minPlayers = 2;
    [SerializeField] private int maxPlayers = 4;
    [SerializeField] PlayerInputManager inputManager;
    [SerializeField] VoidEventSO gameEnd;
    [SerializeField] int gameplaySceneIndex = 1;

    private int currentMinPlayers;
    private int currentMaxPlayers;

    private List<PlayerConfig> playerConfigs;

    public bool singleplayer = false;
   
    public static PlayerConfigManager Instance { get; private set; }

    public MMF_Player FadeEffect;

    private void OnEnable()
    {
        gameEnd.onEventRaised += DestroyThis;
    }

    private void OnDisable()
    {
        gameEnd.onEventRaised -= DestroyThis;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("PlayerConfigManager Singleton is attempting to create more than one instance!");
            return;
        }

        Instance = this;
        DontDestroyOnLoad(Instance);
        playerConfigs = new List<PlayerConfig>();

        currentMaxPlayers = maxPlayers;
        currentMinPlayers = minPlayers;
        inputManager.DisableJoining();
    }


    public void SetPlayerColor(int index, Color color)
    {
        playerConfigs[index].PlayerColor = color;
    }

    public async void SetPlayerReady(int index)
    {
        playerConfigs[index].IsReady = true;
        if (AudioManager.Instance != null)
        { 
            AudioManager.Instance.uiReady();
        }

        if (playerConfigs.Count >= currentMinPlayers && playerConfigs.All(p => p.IsReady == true))
        {
            inputManager.DisableJoining();
            FadeEffect?.PlayFeedbacks();

            await Task.Delay(1000);

            SceneManager.LoadScene(gameplaySceneIndex);
        }
    }

    public void OnPlayerJoined(PlayerInput inp)
    {

        if (playerConfigs == null) return;
        if (AudioManager.Instance != null)
        { 
            AudioManager.Instance.uiJoin();
        }

        if (!playerConfigs.Any(p => p.Input == inp))
        {
            Debug.Log($"Player {inp.playerIndex + 1} Joined");
            playerConfigs.Add(new PlayerConfig(inp));
            inp.transform.SetParent(transform);

            if (playerConfigs.Count >= currentMaxPlayers)
            {
                inputManager.DisableJoining();
            }
        }
    }

    public void ConfigureSingleplayer()
    {
        currentMinPlayers = 1;
        currentMaxPlayers = 1;

        singleplayer = true;

        inputManager.EnableJoining();
    }

    public void ConfigureMultiplayer()
    {
        currentMaxPlayers = maxPlayers;
        currentMinPlayers = minPlayers;

        singleplayer = false;

        inputManager.EnableJoining();
    }

    public List<PlayerConfig> GetPlayerConfigs()
    {
        return playerConfigs;
    }

    void DestroyThis()
    {
        Destroy(gameObject);
    }

}

public class PlayerConfig
{
    public PlayerConfig(PlayerInput inp) {
        PlayerIndex = inp.playerIndex;
        Input = inp;
    }

    public int PlayerIndex { get; set; }
    public PlayerInput Input { get; set; }
    public GameObject MeshPrefab {  get; set; }
    public string CharacterName {  get; set; }
    public bool IsReady { get; set; }
    public Color PlayerColor { get; set; }
    public int RoundsWon { get; set; }

}
