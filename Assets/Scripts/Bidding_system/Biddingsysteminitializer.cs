using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BiddingSystemInitializer : MonoBehaviour
{
    [Header("Bidding System")]
    [SerializeField] private MaskBiddingController biddingController;
    [SerializeField] private BidChooser bidChooser;
    
    [Header("Player Setup")]
    [SerializeField] private Transform playerContainer;
    [SerializeField] private bool autoFindPlayers = true;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private List<PlayerGold> players = new List<PlayerGold>();
    private List<PlayerBiddingInput> inputs = new List<PlayerBiddingInput>();

    private void Start()
    {
        StartCoroutine(InitializeDelayed());
    }

    private IEnumerator InitializeDelayed()
    {
        yield return new WaitForEndOfFrame();
        Initialize();
    }

    public void Initialize()
    {
        if (debugMode) Debug.Log("=== Initializing Bidding System ===");

        if (!ValidateReferences()) return;
        
        FindPlayers();
        
        if (players.Count == 0)
        {
            Debug.LogError("No players found!");
            return;
        }

        SetupSystem();
        
        if (debugMode) Debug.Log($"=== Initialized with {players.Count} players ===");
    }

    private bool ValidateReferences()
    {
        if (biddingController == null)
            biddingController = FindObjectOfType<MaskBiddingController>();
            
        if (biddingController == null)
        {
            Debug.LogError("No MaskBiddingController found!");
            return false;
        }

        if (bidChooser == null)
        {
            bidChooser = FindObjectOfType<BidChooser>();
            if (debugMode && bidChooser == null)
                Debug.LogWarning("No BidChooser found (optional)");
        }

        return true;
    }

    private void FindPlayers()
    {
        players.Clear();
        inputs.Clear();

        PlayerGold[] foundPlayers = autoFindPlayers 
            ? FindObjectsOfType<PlayerGold>()
            : playerContainer?.GetComponentsInChildren<PlayerGold>() ?? new PlayerGold[0];

        if (debugMode) Debug.Log($"Found {foundPlayers.Length} players");

        foreach (var player in foundPlayers)
        {
            players.Add(player);
            
            // Get or add input component
            var input = player.GetComponent<PlayerBiddingInput>() 
                     ?? player.gameObject.AddComponent<PlayerBiddingInput>();
            inputs.Add(input);
        }

        // Sort by player index
        players.Sort((a, b) => a.PlayerIndex.CompareTo(b.PlayerIndex));
        
        if (debugMode)
            players.ForEach(p => Debug.Log($"  Player {p.PlayerIndex} ({p.name})"));
    }

    private void SetupSystem()
    {
        // Setup controller
        biddingController.players = new List<PlayerGold>(players);
        
        // Setup inputs
        var configs = PlayerConfigManager.Instance?.GetPlayerConfigs().ToArray();
        for (int i = 0; i < inputs.Count; i++)
        {
            var input = inputs[i];
            var player = players[i];

            input.SetBiddingController(biddingController);
            if (bidChooser != null) input.SetBidChooser(bidChooser);

            // Initialize with config or fallback to PlayerInput
            if (configs != null && i < configs.Length)
            {
                input.Initialize(configs[i]);
            }
            else
            {
                var playerInput = player.GetComponent<PlayerInput>() 
                               ?? player.GetComponentInChildren<PlayerInput>();
                
                if (playerInput != null)
                    input.InitializeWithPlayerInput(playerInput, player.PlayerIndex);
                else
                    Debug.LogWarning($"No PlayerInput for Player {player.PlayerIndex}");
            }
        }

        // Setup bid chooser
        if (bidChooser != null)
        {
            bidChooser.players = new List<PlayerGold>(players);
            bidChooser.Initialize();
        }
    }

    public void RegisterPlayer(PlayerGold player, PlayerBiddingInput input = null)
    {
        if (players.Contains(player)) return;

        players.Add(player);
        
        input = input ?? player.GetComponent<PlayerBiddingInput>() 
                     ?? player.gameObject.AddComponent<PlayerBiddingInput>();
        inputs.Add(input);
        
        // Update references
        biddingController.players = new List<PlayerGold>(players);
        if (bidChooser != null) bidChooser.players = new List<PlayerGold>(players);
        
        if (debugMode) Debug.Log($"Registered Player {player.PlayerIndex}");
    }

    public void StartMaskPhase()
    {
        biddingController?.BeginMaskPhase();
        inputs.ForEach(i => i.SetBiddingPhase(true));
        if (debugMode) Debug.Log("Started Mask Phase");
    }

    public void StartTarotPhase()
    {
        biddingController?.BeginTarotPhase();
        inputs.ForEach(i => i.SetBiddingPhase(false));
        if (debugMode) Debug.Log("Started Tarot Phase");
    }

    [ContextMenu("Test Initialize")]
    private void TestInitialize() => Initialize();

    [ContextMenu("Test Start Mask Phase")]
    private void TestStartMaskPhase() => StartMaskPhase();

    [ContextMenu("Test Start Tarot Phase")]
    private void TestStartTarotPhase() => StartTarotPhase();
}