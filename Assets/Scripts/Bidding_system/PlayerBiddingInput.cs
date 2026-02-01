using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBiddingInput : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerGold playerGold;
    [SerializeField] private MaskBiddingController biddingController;
    [SerializeField] private BidChooser bidChooser;
    
    [Header("Input Settings")]
    [SerializeField] private int bidIncrement = 10;
    [SerializeField] private float repeatDelay = 0.15f;
    [SerializeField] private float initialRepeatDelay = 0.3f;
    
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction confirmAction;
    private InputAction cancelAction;
    
    private float verticalRepeatTimer;
    private bool isVerticalHeld;
    private int lastVerticalDirection;
    
    private int playerIndex;
    private bool isInitialized = false;
    private bool canBid = true;
    private int currentBidAmount = 10;
    private bool isMaskPhase = false;
    
    private void Awake()
    {
        playerGold ??= GetComponent<PlayerGold>();
        bidChooser ??= FindObjectOfType<BidChooser>(true);
        biddingController ??= FindObjectOfType<MaskBiddingController>();
    }
    
    public void Initialize(PlayerConfig config)
    {
        FindPlayerInput();
        
        if (playerInput != null)
        {
            playerIndex = config.PlayerIndex;
            SetupInputActions();
            isInitialized = true;
            Debug.Log($"Player {playerIndex} bidding input initialized");
        }
        else
        {
            Debug.LogError($"Player {config.PlayerIndex} could not find PlayerInput!");
        }
    }
    
    public void InitializeWithPlayerInput(PlayerInput input, int index)
    {
        playerInput = input;
        playerIndex = index;
        SetupInputActions();
        isInitialized = true;
        Debug.Log($"Player {playerIndex} bidding input initialized manually");
    }

    private void FindPlayerInput()
    {
        if (playerInput != null) return;
        
        playerInput = GetComponent<PlayerInput>() 
                   ?? GetComponentInChildren<PlayerInput>() 
                   ?? GetComponentInParent<PlayerInput>();
    }
    
    public void SetBiddingPhase(bool isMask)
    {
        isMaskPhase = isMask;
        canBid = true;
        currentBidAmount = biddingController?.CurrentBidAmount ?? 10;
        
        Debug.Log($"Player {playerIndex}: {(isMask ? "Mask" : "Tarot")} phase, starting bid: {currentBidAmount}");
    }
    
    public void SetCanBid(bool value) => canBid = value;
    
    private void SetupInputActions()
    {
        if (playerInput == null) return;
        
        moveAction = playerInput.actions["Move"];
        confirmAction = playerInput.actions["Submit"];
        cancelAction = playerInput.actions["Cancel"];
        
        if (moveAction != null)
        {
            moveAction.started += OnMoveStarted;
            moveAction.canceled += OnMoveCanceled;
        }
        
        if (confirmAction != null)
            confirmAction.performed += OnConfirm;
            
        if (cancelAction != null)
            cancelAction.performed += OnCancel;
    }
    
    private void Update()
    {
        if (!IsActive()) return;
        
        HandleVerticalRepeat();
        EnforceMinimumBid();
    }
    
    private bool IsActive() => isInitialized && canBid && 
                               (biddingController?.IsMaskPhase == true || 
                                biddingController?.IsTarotPhase == true);
    
    private void EnforceMinimumBid()
    {
        int minimum = biddingController?.CurrentBidAmount ?? currentBidAmount;
        if (currentBidAmount < minimum)
        {
            currentBidAmount = minimum;
            Debug.Log($"Player {playerIndex} bid forced to minimum: {currentBidAmount}");
        }
    }
    
    private void HandleVerticalRepeat()
    {
        if (!isVerticalHeld) return;
        
        verticalRepeatTimer -= Time.deltaTime;
        if (verticalRepeatTimer <= 0)
        {
            AdjustBid(lastVerticalDirection);
            verticalRepeatTimer = repeatDelay;
        }
    }
    
    private void OnMoveStarted(InputAction.CallbackContext context)
    {
        if (!IsActive()) return;
            
        Vector2 move = context.ReadValue<Vector2>();
        
        if (Mathf.Abs(move.y) > 0.5f)
        {
            isVerticalHeld = true;
            lastVerticalDirection = move.y > 0 ? 1 : -1;
            verticalRepeatTimer = initialRepeatDelay;
            AdjustBid(lastVerticalDirection);
        }
    }
    
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        isVerticalHeld = false;
        lastVerticalDirection = 0;
    }
    
    private void OnConfirm(InputAction.CallbackContext context)
    {
        if (!IsActive()) return;
        
        if (isMaskPhase && biddingController.IsMaskPhase)
            PlaceMaskBid();
        else if (!isMaskPhase && biddingController.IsTarotPhase)
            PlaceTarotBid();
    }
    
    private void OnCancel(InputAction.CallbackContext context)
    {
        if (!IsActive()) return;
        
        if (isMaskPhase && biddingController.IsMaskPhase)
            TakeMaskWithoutBid();
        else if (!isMaskPhase && biddingController.IsTarotPhase)
            PassOnCurrentTarot();
    }
    
    private void PlaceMaskBid()
    {
        if (!ValidateBid()) return;
        
        int minimum = biddingController.CurrentBidAmount;
        currentBidAmount = Mathf.Max(currentBidAmount, minimum);
        
        biddingController.PlayerBid(playerGold, currentBidAmount);
        SyncBidChooser();
        
        bool raising = currentBidAmount > minimum;
        Debug.Log($"Player {playerIndex} bid {currentBidAmount} to AVOID mask{(raising ? " (RAISING!)" : "")}");
        
        canBid = false;
    }
    
    private void PlaceTarotBid()
    {
        if (!ValidateBid()) return;
        
        int minimum = biddingController.CurrentBidAmount;
        currentBidAmount = Mathf.Max(currentBidAmount, minimum);
        
        biddingController.PlayerBid(playerGold, currentBidAmount);
        SyncBidChooser();
        
        bool raising = currentBidAmount > minimum;
        Debug.Log($"Player {playerIndex} bid {currentBidAmount} to WIN tarot{(raising ? " (RAISING!)" : "")}");
        
        canBid = false;
    }
    
    private bool ValidateBid()
    {
        if (playerGold == null || biddingController == null)
        {
            Debug.LogError($"Player {playerIndex} missing components!");
            return false;
        }
        
        if (!playerGold.CanAfford(currentBidAmount))
        {
            Debug.Log($"Player {playerIndex} cannot afford {currentBidAmount}");
            return false;
        }
        
        return true;
    }
    
    private void TakeMaskWithoutBid()
    {
        Debug.Log($"Player {playerIndex} taking mask without bidding");
        biddingController?.TakeMaskWithoutBid(playerIndex);
        canBid = false;
    }
    
    private void PassOnCurrentTarot()
    {
        Debug.Log($"Player {playerIndex} passing on tarot (kicked from round)");
        biddingController?.PassOnCurrentItem(playerIndex);
        canBid = false;
    }
    
    private void AdjustBid(int direction)
    {
        if (biddingController == null) return;
        
        int minimum = biddingController.CurrentBidAmount;
        
        if (direction > 0)
        {
            currentBidAmount += bidIncrement;
            Debug.Log($"Player {playerIndex} increased bid to {currentBidAmount}");
        }
        else if (direction < 0)
        {
            int newAmount = currentBidAmount - bidIncrement;
            if (newAmount >= minimum)
            {
                currentBidAmount = newAmount;
                Debug.Log($"Player {playerIndex} decreased bid to {currentBidAmount}");
            }
            else
            {
                Debug.Log($"Player {playerIndex} cannot bid below minimum ({minimum})");
            }
        }
        
        SyncBidChooser();
    }
    
    private void SyncBidChooser()
    {
        bidChooser?.SetCurrentBid(currentBidAmount);
    }
    
    // Public API
    public void SetBidChooser(BidChooser chooser) => bidChooser = chooser;
    public void SetBiddingController(MaskBiddingController controller) => biddingController = controller;
    public int GetPlayerIndex() => playerIndex;
    public int GetCurrentBid() => currentBidAmount;
    public bool IsBiddingActive() => canBid && isInitialized;
    
    public void ResetForNewRound()
    {
        canBid = true;
        isVerticalHeld = false;
        lastVerticalDirection = 0;
        currentBidAmount = biddingController?.CurrentBidAmount ?? 10;
        
        Debug.Log($"Player {playerIndex} reset for new round, bid: {currentBidAmount}");
    }
    
    public void ResetInput()
    {
        isMaskPhase = false;
        ResetForNewRound();
    }
    
    private void OnDestroy()
    {
        if (moveAction != null)
        {
            moveAction.started -= OnMoveStarted;
            moveAction.canceled -= OnMoveCanceled;
        }
        
        if (confirmAction != null) confirmAction.performed -= OnConfirm;
        if (cancelAction != null) cancelAction.performed -= OnCancel;
    }
}