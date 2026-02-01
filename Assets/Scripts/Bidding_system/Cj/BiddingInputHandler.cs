using MoreMountains.Feedbacks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class BiddingInputHandler : MonoBehaviour
{
    public int playerIndex { get => cfg.PlayerIndex; }
    [SerializeField] TextMeshProUGUI bidNumberTMP;
    [SerializeField] TextMeshProUGUI acornsTMP;
    [SerializeField] GameObject turnIndicator;

    public MMF_Player TicUpEffect;
    public MMF_Player TicDownEffect;
    public MMF_Player ShakeEffect;
    public MMF_Player BMEffect;
    public MMF_Player SMEffect;

    private BiddingManager manager;
    public PlayerConfig cfg;
    private Player controls;

    int currentlyBidding = 10;
    public int currentBidRequirement = 10;

    float canInputCD = .15f;
    float canInputTimer = 0;

    public bool IsTurn = false;

    private void Awake()
    {
        turnIndicator.SetActive(false);
        controls = new Player();
        manager = FindAnyObjectByType<BiddingManager>();
    }

    public void Initialize(PlayerConfig cfg)
    {
        this.cfg = cfg;

        cfg.Input.onActionTriggered += OnActionTriggered;
        cfg.Input.SwitchCurrentActionMap("PlayerBidding");

        cfg.Acorns = 300;

        UpdatePlayerUI();
    }

    private void OnDestroy()
    {
        cfg.Input.onActionTriggered -= OnActionTriggered;
    }

    public void OnActionTriggered(InputAction.CallbackContext context)
    {
        if (canInputTimer > 0) return;

        if (context.action.name == controls.PlayerBidding.Bid.name) OnBid(context);
        if (context.action.name == controls.PlayerBidding.IncreaseBid.name) OnIncreaseBid(context);
        if (context.action.name == controls.PlayerBidding.DecreaseBid.name) OnDecreaseBid(context);
        if (context.action.name == controls.PlayerBidding.Take.name) OnTake(context);
    }

    private void OnDecreaseBid(InputAction.CallbackContext context)
    {
        canInputTimer = canInputCD;
        currentlyBidding -= 10;

        if (currentlyBidding < currentBidRequirement)
        {
            currentlyBidding += 10;
        }
        {
            TicDownEffect?.PlayFeedbacks();
            AudioManager.Instance.uiTickDown();
        }

    }

    private void OnIncreaseBid(InputAction.CallbackContext context)
    {
        canInputTimer = canInputCD;
        currentlyBidding += 10;

        if (currentlyBidding > cfg.Acorns)
        {
            currentlyBidding -= 10;
        }
        else
        {
            TicUpEffect?.PlayFeedbacks();
            AudioManager.Instance.uiTickUp();
        }
    }

    private void OnBid(InputAction.CallbackContext context)
    {
        canInputTimer = canInputCD;
        SMEffect?.PlayFeedbacks();
        AudioManager.Instance.uiBet();
        EndTurn(false, currentlyBidding);
    }

    private void OnTake(InputAction.CallbackContext context)
    {
        canInputTimer = canInputCD;
        BMEffect?.PlayFeedbacks();
        AudioManager.Instance.uiTake();
        EndTurn(true, currentlyBidding);
    }

    void EndTurn(bool passedTurn, int acornBidAmount)
    {
        IsTurn = false;
        manager.SetTurnContext(new TurnContext(passedTurn, acornBidAmount, this));
        ShakeEffect?.PlayFeedbacks();
    }

    public void OnTurnEnter(int reqAmount)
    {
        IsTurn = true;
        currentBidRequirement = reqAmount;

        currentlyBidding = currentBidRequirement;

        if (cfg.Acorns < reqAmount) EndTurn(true, currentlyBidding);

    }

    void UpdatePlayerUI()
    {
        acornsTMP.text = $"{cfg.Acorns}";
        bidNumberTMP.text = $"{currentlyBidding}";

        turnIndicator.SetActive(IsTurn);

    }

    private void Update()
    {
        if (canInputCD > 0) canInputTimer -= Time.deltaTime;

        UpdatePlayerUI();
    }
}

[Serializable]
public class TurnContext
{
    public bool passedTurn;
    public int acornBidAmount;
    public BiddingInputHandler player;

    public TurnContext(bool passedTurn, int acornBidAmount, BiddingInputHandler player)
    {
        this.passedTurn = passedTurn;
        this.acornBidAmount = acornBidAmount;
        this.player = player;
    }
}
