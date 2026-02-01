using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BiddingSystemInitializer : MonoBehaviour
{
    [Header("Bidding System")]
    [SerializeField] private MaskBiddingController biddingController;
    [SerializeField] private BidChooser bidChooser;
    
    [Header("Turn Management")]
    [SerializeField] private TurnManager turnManager;

    [Header("Player Setup")]
    [SerializeField] private GameObject biddingPlayerPrefab;
    [SerializeField] private Transform playerContainer;
    [SerializeField] private bool autoFindPlayers = true;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private List<PlayerGold> players = new List<PlayerGold>();
    private List<PlayerBiddingInput> playerInputs = new List<PlayerBiddingInput>();

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
        // Find MaskBiddingController
        if (biddingController == null)
        {
            biddingController = FindObjectOfType<MaskBiddingController>();
            if (debugMode) Debug.Log($"Found controller: {biddingController != null}");
        }
            
        if (biddingController == null)
        {
            Debug.LogError("No MaskBiddingController found!");
            return false;
        }

        // Find BidChooser
        if (bidChooser == null)
        {
            bidChooser = FindObjectOfType<BidChooser>(true);
            if (debugMode) Debug.Log($"Found bid chooser: {bidChooser != null}");
        }

        // Create or find TurnManager
        if (turnManager == null)
        {
            turnManager = FindObjectOfType<TurnManager>();
            if (turnManager == null)
            {
                if (debugMode) Debug.Log("Creating new TurnManager");
                GameObject turnManagerObj = new GameObject("TurnManager");
                turnManager = turnManagerObj.AddComponent<TurnManager>();
                
                // Parent it to this object for organization
                turnManagerObj.transform.SetParent(transform);
            }
        }

        return true;
    }

    private void FindPlayers()
    {
        players.Clear();
        playerInputs.Clear();

        PlayerGold[] foundPlayers = autoFindPlayers 
            ? FindObjectsOfType<PlayerGold>()
            : playerContainer?.GetComponentsInChildren<PlayerGold>() ?? new PlayerGold[0];

        if (debugMode) Debug.Log($"Found {foundPlayers.Length} players");

        foreach (var player in foundPlayers)
        {
            players.Add(player);
            
            // Get or add PlayerBiddingInput component
            var input = player.GetComponent<PlayerBiddingInput>() 
                     ?? player.gameObject.AddComponent<PlayerBiddingInput>();
            playerInputs.Add(input);
            
            if (debugMode) Debug.Log($"  Added Player {player.PlayerIndex}: {player.name}");
        }

        // Sort by player index
        players.Sort((a, b) => a.PlayerIndex.CompareTo(b.PlayerIndex));
        playerInputs.Sort((a, b) => a.GetPlayerIndex().CompareTo(b.GetPlayerIndex()));
    }

    private void SetupSystem()
    {
        // Setup MaskBiddingController with players
        biddingController.players = new List<PlayerGold>(players);
        
        // Setup TurnManager
        turnManager.Initialize(players, playerInputs, biddingController, bidChooser);
        
        // Setup PlayerInputs
        var configs = PlayerConfigManager.Instance?.GetPlayerConfigs().ToArray();
        
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            var input = playerInputs[i];
            
            if (debugMode) Debug.Log($"Setting up Player {player.PlayerIndex}");

            // Set references
            input.SetBiddingController(biddingController);
            if (bidChooser != null) input.SetBidChooser(bidChooser);
            
            // Initialize with PlayerConfig or fallback
            if (configs != null && i < configs.Length)
            {
                if (debugMode) Debug.Log($"  Initializing with PlayerConfig {configs[i].PlayerIndex}");
                input.Initialize(configs[i]);
            }
            else
            {
                // Fallback: try to find PlayerInput component
                var playerInput = player.GetComponent<PlayerInput>() 
                               ?? player.GetComponentInChildren<PlayerInput>();
                
                if (playerInput != null)
                {
                    if (debugMode) Debug.Log($"  Initializing with PlayerInput");
                    input.InitializeWithPlayerInput(playerInput, player.PlayerIndex);
                }
                else
                {
                    Debug.LogWarning($"No PlayerInput found for Player {player.PlayerIndex}");
                    // Still initialize but without input
                    input.InitializeWithPlayerInput(null, player.PlayerIndex);
                }
            }
            
            // Register with TurnManager
            turnManager.RegisterPlayer(input, player.PlayerIndex);
        }

        // Setup BidChooser
        if (bidChooser != null)
        {
            bidChooser.players = new List<PlayerGold>(players);
            bidChooser.turnManager = turnManager; // Connect turn manager
            
            // If bid chooser has UI button callbacks, they'll work alongside controller input
            bidChooser.Initialize();
            
            if (debugMode) Debug.Log("BidChooser initialized");
        }
        
        // Setup event connections between BidChooser and TurnManager
        if (bidChooser != null && turnManager != null)
        {
            // BidChooser will call turnManager when it moves to a player
            // This is handled in MovePanelToPlayer method
        }
        
        // Initialize all players to not their turn
        foreach (var input in playerInputs)
        {
            input.SetIsMyTurn(false);
            input.SetBiddingPhaseActive(false);
        }
        
        if (debugMode) Debug.Log($"System setup complete with {players.Count} players");
    }

    public void RegisterPlayer(PlayerGold player, PlayerBiddingInput input = null)
    {
        if (players.Contains(player)) 
        {
            if (debugMode) Debug.Log($"Player {player.PlayerIndex} already registered");
            return;
        }

        players.Add(player);
        
        input = input ?? player.GetComponent<PlayerBiddingInput>() 
                     ?? player.gameObject.AddComponent<PlayerBiddingInput>();
        playerInputs.Add(input);
        
        // Update references
        biddingController.players = new List<PlayerGold>(players);
        if (bidChooser != null) 
        {
            bidChooser.players = new List<PlayerGold>(players);
        }
        
        // Register with TurnManager
        if (turnManager != null)
        {
            turnManager.RegisterPlayer(input, player.PlayerIndex);
        }
        
        if (debugMode) Debug.Log($"Registered Player {player.PlayerIndex}");
    }

    public void UnregisterPlayer(PlayerGold player)
    {
        if (!players.Contains(player)) return;
        
        int index = players.IndexOf(player);
        players.Remove(player);
        
        if (index < playerInputs.Count)
        {
            playerInputs.RemoveAt(index);
        }
        
        // Update references
        biddingController.players = new List<PlayerGold>(players);
        if (bidChooser != null) 
        {
            bidChooser.players = new List<PlayerGold>(players);
        }
        
        if (debugMode) Debug.Log($"Unregistered Player {player.PlayerIndex}");
    }

    public void StartMaskPhase()
    {
        if (debugMode) Debug.Log("Starting Mask Phase");
        
        biddingController?.BeginMaskPhase();
        
        // Tell TurnManager to start mask phase
        if (turnManager != null)
        {
            turnManager.StartMaskPhase();
        }
        else
        {
            // Fallback: enable all inputs if no turn manager
            foreach (var input in playerInputs)
            {
                input.SetBiddingPhase(true);
            }
        }
    }

    public void StartTarotPhase()
    {
        if (debugMode) Debug.Log("Starting Tarot Phase");
        
        biddingController?.BeginTarotPhase();
        
        // Tell TurnManager to start tarot phase
        if (turnManager != null)
        {
            turnManager.StartTarotPhase();
        }
        else
        {
            // Fallback: enable all inputs if no turn manager
            foreach (var input in playerInputs)
            {
                input.SetBiddingPhase(false);
            }
        }
    }

    public void EnableAllPlayersInput()
    {
        if (debugMode) Debug.Log("Enabling all player inputs");
        
        if (turnManager != null)
        {
            turnManager.EnableAllPlayers();
        }
        else
        {
            foreach (var input in playerInputs)
            {
                input.SetIsMyTurn(true);
                input.SetBiddingPhaseActive(true);
            }
        }
    }

    public void EnableTurnBasedInput()
    {
        if (debugMode) Debug.Log("Enabling turn-based input");
        
        if (turnManager != null)
        {
            turnManager.EnableTurnBased();
        }
    }

    public List<PlayerGold> GetPlayers() => new List<PlayerGold>(players);
    public List<PlayerBiddingInput> GetPlayerInputs() => new List<PlayerBiddingInput>(playerInputs);
    public TurnManager GetTurnManager() => turnManager;

    [ContextMenu("Test Initialize")]
    private void TestInitialize() => Initialize();

    [ContextMenu("Test Start Mask Phase")]
    private void TestStartMaskPhase() => StartMaskPhase();

    [ContextMenu("Test Start Tarot Phase")]
    private void TestStartTarotPhase() => StartTarotPhase();

    [ContextMenu("Test Enable All Players")]
    private void TestEnableAllPlayers() => EnableAllPlayersInput();

    [ContextMenu("Test Enable Turn Based")]
    private void TestEnableTurnBased() => EnableTurnBasedInput();
}
