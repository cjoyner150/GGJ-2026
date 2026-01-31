using System;
using System.Collections.Generic;
using UnityEngine;

public class MaskBiddingController : MonoBehaviour
{
    // Events for UI updates
    public event Action<PlayerGold, Mask> OnMaskAssigned;
    public event Action<PlayerGold, TarotCard> OnTarotAssigned;
    public event Action<int> OnPotUpdated;
    public event Action<float> OnTimerUpdated;
    public event Action<Mask> OnMaskDrawn;
    public event Action<TarotCard> OnTarotDrawn;
    public event Action<PlayerGold> OnPlayerBidPlaced;
    public event Action OnBiddingFinished;
    public event Action OnMaskPhaseStarted;
    public event Action OnTarotPhaseStarted;

    [Header("Mask Bidding")]
    public int maskBidCost = 10;
    public List<Mask> masks;
    public int maskBidTimer = 5; // Seconds per turn

    [Header("Tarot Bidding")]
    public int tarotBidIncrement = 10;
    public List<TarotCard> tarotCards;
    public int tarotBidTimer = 5; // Seconds per turn

    [Header("Game Rules")]
    public int startingPot = 0;
    public bool autoDrawNextMask = true;
    public float maskDrawDelay = 1f;

    // Internal state
    private MaskDeck maskDeck;
    private BiddingSession maskSession;
    private NormalBiddingSession tarotSession;
    private int currentPot = 0;
    private float currentTimer = 0f;
    private int maskIndex = 0;
    private bool isTimerRunning = false;
    private Coroutine maskDrawCoroutine;

    // Public properties
    public bool IsMaskPhase { get; private set; }
    public bool IsTarotPhase { get; private set; }
    public int CurrentPot => currentPot;
    public float CurrentTimer => currentTimer;
    public Mask CurrentMask { get; private set; }
    public TarotCard CurrentTarot { get; private set; }

    void Start()
    {
        InitializeDeck();
        currentPot = startingPot;
    }

    void Update()
    {
        if (isTimerRunning && currentTimer > 0)
        {
            currentTimer -= Time.deltaTime;
            OnTimerUpdated?.Invoke(currentTimer);

            if (currentTimer <= 0)
            {
                OnTimerExpired();
            }
        }
    }

    void InitializeDeck()
    {
        if (masks != null && masks.Count > 0)
        {
            maskDeck = new MaskDeck(masks);
        }
        else
        {
            Debug.LogWarning("No masks assigned to MaskBiddingController!");
        }
    }

    public void BeginMaskPhase()
    {
        if (masks == null || masks.Count == 0)
        {
            Debug.LogError("Cannot begin mask phase: No masks assigned!");
            return;
        }

        IsMaskPhase = true;
        IsTarotPhase = false;
        maskIndex = 0;

        OnMaskPhaseStarted?.Invoke();
        StartNextMask();
    }

    public void BeginTarotPhase()
    {
        IsMaskPhase = false;
        IsTarotPhase = true;

        OnTarotPhaseStarted?.Invoke();
        Debug.Log("Tarot phase started");
    }

    void StartNextMask()
    {
        if (maskIndex >= masks.Count)
        {
            Debug.Log("All masks have been assigned");
            OnBiddingFinished?.Invoke();
            return;
        }

        if (maskDeck == null)
        {
            Debug.LogError("Mask deck not initialized!");
            return;
        }

        CurrentMask = maskDeck.Draw();
        maskIndex++;

        OnMaskDrawn?.Invoke(CurrentMask);
        StartTimer(maskBidTimer);

        Debug.Log($"Drawing mask {maskIndex}/{masks.Count}: {CurrentMask.description}");
    }

    public void PlayerBid(PlayerGold player)
    {
        if (player == null) return;

        int bidAmount = IsMaskPhase ? maskBidCost : tarotBidIncrement;

        if (player.TrySpend(bidAmount))
        {
            // Update pot
            currentPot += bidAmount;
            OnPotUpdated?.Invoke(currentPot);

            // Notify listeners
            OnPlayerBidPlaced?.Invoke(player);

            // Handle mask phase logic
            if (IsMaskPhase)
            {
                if (maskSession == null)
                {
                    Debug.LogError("Mask session not initialized!");
                    return;
                }

                maskSession.TryBid(player);

                // Check if we should draw next mask
                if (autoDrawNextMask)
                {
                    RestartTimer();
                }
            }
            // Handle tarot phase logic
            else if (IsTarotPhase)
            {
                if (tarotSession == null)
                {
                    tarotSession = new NormalBiddingSession(tarotBidIncrement);
                }

                tarotSession.TryBid(player);
                RestartTimer();
            }
        }
        else
        {
            Debug.Log($"Player {player.PlayerIndex} cannot afford {bidAmount} gold");
        }
    }

    void StartTimer(int seconds)
    {
        currentTimer = seconds;
        isTimerRunning = true;
        OnTimerUpdated?.Invoke(currentTimer);
    }

    void RestartTimer()
    {
        int timerLength = IsMaskPhase ? maskBidTimer : tarotBidTimer;
        StartTimer(timerLength);
    }

    void OnTimerExpired()
    {
        isTimerRunning = false;
        currentTimer = 0f;
        OnTimerUpdated?.Invoke(0f);

        Debug.Log("Bidding timer expired!");

        // In a real game, you might auto-pass or auto-bid here
        // For now, just notify
        if (IsMaskPhase && autoDrawNextMask)
        {
            // Auto-draw next mask after delay
            if (maskDrawCoroutine != null)
                StopCoroutine(maskDrawCoroutine);
            
            maskDrawCoroutine = StartCoroutine(DrawNextMaskAfterDelay());
        }
    }

    System.Collections.IEnumerator DrawNextMaskAfterDelay()
    {
        yield return new WaitForSeconds(maskDrawDelay);
        StartNextMask();
    }

    public void AssignMaskToPlayer(PlayerGold player)
    {
        if (CurrentMask == null)
        {
            Debug.LogError("No current mask to assign!");
            return;
        }

        OnMaskAssigned?.Invoke(player, CurrentMask);
        Debug.Log($"Assigned mask to Player {player.PlayerIndex}: {CurrentMask.description}");

        // Draw next mask
        if (autoDrawNextMask)
        {
            StartNextMask();
        }
    }

    public void DrawRandomTarot()
    {
        if (tarotCards == null || tarotCards.Count == 0)
        {
            Debug.LogError("No tarot cards available!");
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, tarotCards.Count);
        CurrentTarot = tarotCards[randomIndex];

        OnTarotDrawn?.Invoke(CurrentTarot);
        StartTimer(tarotBidTimer);

        Debug.Log($"Drawn tarot card: {CurrentTarot.name}");
    }

    public void AssignTarotToWinner(PlayerGold winner)
    {
        if (CurrentTarot == null)
        {
            Debug.LogError("No current tarot card to assign!");
            return;
        }

        OnTarotAssigned?.Invoke(winner, CurrentTarot);
        Debug.Log($"Assigned tarot card to Player {winner.PlayerIndex}: {CurrentTarot.name}");
    }

    public void EndTarotBidding()
    {
        if (tarotSession != null)
        {
            tarotSession.EndBidding();
        }
    }

    public void AddToPot(int amount)
    {
        currentPot += amount;
        OnPotUpdated?.Invoke(currentPot);
    }

    public void ResetPot()
    {
        currentPot = startingPot;
        OnPotUpdated?.Invoke(currentPot);
    }

    public void SetBidCost(int newCost, bool isMaskPhase = true)
    {
        if (isMaskPhase)
        {
            maskBidCost = Mathf.Max(10, newCost);
        }
        else
        {
            tarotBidIncrement = Mathf.Max(10, newCost);
        }
    }

    // Test methods
    public void TestDrawMask()
    {
        if (masks.Count > 0)
        {
            BeginMaskPhase();
        }
    }

    public void TestDrawTarot()
    {
        if (tarotCards.Count > 0)
        {
            BeginTarotPhase();
            DrawRandomTarot();
        }
    }

    public void ForceTimerExpire()
    {
        OnTimerExpired();
    }
}