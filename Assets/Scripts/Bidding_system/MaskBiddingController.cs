using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class MaskBiddingController : MonoBehaviour
{
    // Consolidated events
    public event Action<PlayerGold, Mask> OnMaskAssigned;
    public event Action<PlayerGold, TarotCard> OnTarotAssigned;
    public event Action<int> OnPotUpdated;
    public event Action<float> OnTimerUpdated;
    public event Action<Mask> OnMaskDrawn;
    public event Action<TarotCard> OnTarotDrawn;
    public event Action<PlayerGold> OnPlayerBidPlaced;
    public event Action OnMaskPhaseStarted;
    public event Action OnTarotPhaseStarted;
    public event Action<int> OnCurrentBidAmountChanged;
    public event Action<PlayerGold> OnPlayerKickedFromRound;
    public event Action OnBiddingFinished;

    [Header("Mask Bidding")]
    public int maskBidCost = 10;
    public List<Mask> masks;
    public int maskBidTimer = 10;

    [Header("Tarot Bidding")]
    public int tarotBidIncrement = 10;
    public List<TarotCard> tarotCards;
    public int tarotBidTimer = 10;
    private int tarotsDrawnThisRound = 0;
    public int tarotsPerRound = 3;

    [Header("Game Rules")]
    public int startingPot = 0;
    public bool autoDrawNextMask = true;
    public float maskDrawDelay = 1f;

    [Header("Player Setup")]
    public List<PlayerGold> players = new List<PlayerGold>();

    // Internal state
    private MaskDeck maskDeck;
    private BiddingSession maskSession;
    private NormalBiddingSession tarotSession;
    private int currentPot = 0;
    private float currentTimer = 0f;
    private int maskIndex = 0;
    private bool isTimerRunning = false;
    private TarotCard lastDrawnTarot;

    // Public properties
    public bool IsMaskPhase { get; private set; }
    public bool IsTarotPhase { get; private set; }
    public int CurrentPot => currentPot;
    public Mask CurrentMask { get; private set; }
    public TarotCard CurrentTarot { get; private set; }
    public int CurrentBidAmount => IsMaskPhase ? 
        (maskSession?.CurrentBidAmount ?? maskBidCost) : 
        (tarotSession?.CurrentBidAmount ?? tarotBidIncrement);

    void Start()
    {
        if (masks?.Count > 0) maskDeck = new MaskDeck(masks);
        currentPot = startingPot;
        if (players.Count == 0) players = new List<PlayerGold>(FindObjectsOfType<PlayerGold>());
    }

    void Update()
    {
        if (isTimerRunning && currentTimer > 0)
        {
            currentTimer -= Time.deltaTime;
            OnTimerUpdated?.Invoke(currentTimer);
            if (currentTimer <= 0) OnTimerExpired();
        }
    }

    public void BeginMaskPhase()
    {
        if (IsMaskPhase)
        {
            Debug.LogWarning("Already in mask phase! Ignoring duplicate start.");
            return;
        }
        
        if (masks?.Count == 0 || players?.Count == 0)
        {
            Debug.LogError("Cannot begin mask phase: Missing masks or players!");
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
        tarotsDrawnThisRound = 0;
        OnTarotPhaseStarted?.Invoke();
    }

    void StartNextMask()
    {
        if (maskIndex >= masks.Count)
        {
            Debug.Log("All masks assigned - switching to tarot phase");
            IsMaskPhase = false;
            
            // Clear mask session to prevent further mask actions
            maskSession = null;
            
            StartCoroutine(DelayedTarotStart(2f));
            return;
        }

        CurrentMask = maskDeck.Draw();
        maskIndex++;

        if (maskSession == null)
        {
            maskSession = new BiddingSession(players, maskBidCost);
            maskSession.OnMaskAssigned += (player, isBlessed) => AssignMaskToPlayer(player, isBlessed);
            maskSession.OnCurrentBidAmountChanged += amount => OnCurrentBidAmountChanged?.Invoke(amount);
        }
        else
        {
            maskSession.ResetForNewMask();
        }

        OnMaskDrawn?.Invoke(CurrentMask);
        OnCurrentBidAmountChanged?.Invoke(maskBidCost);
        StartTimer(maskBidTimer);
        Debug.Log($"Drew mask {maskIndex}/{masks.Count}: {CurrentMask?.description}");
    }

    public void PlayerBid(PlayerGold player, int bidAmount)
    {
        if (player == null) return;

        bool success = false;
        if (IsMaskPhase && maskSession != null)
        {
            success = maskSession.TryBid(player, bidAmount);
        }
        else if (IsTarotPhase && tarotSession != null)
        {
            success = tarotSession.TryBid(player, bidAmount);
        }

        if (success)
        {
            currentPot += bidAmount;
            OnPotUpdated?.Invoke(currentPot);
            OnPlayerBidPlaced?.Invoke(player);
            if (autoDrawNextMask) RestartTimer();
        }
    }

    void StartTimer(int seconds)
    {
        currentTimer = seconds;
        isTimerRunning = true;
        OnTimerUpdated?.Invoke(currentTimer);
    }

    void RestartTimer() => StartTimer(IsMaskPhase ? maskBidTimer : tarotBidTimer);

    void OnTimerExpired()
    {
        isTimerRunning = false;

        if (IsMaskPhase && maskSession != null)
        {
            if (CurrentMask != null)
            {
                maskSession.ForceLowestBidderToTakeMask();
            }
        }
        else if (IsTarotPhase && tarotSession != null)
        {
            tarotSession.EndBidding();
        }
    }

    void AssignMaskToPlayer(PlayerGold player, bool isBlessed = false)
    {
        if (CurrentMask == null) return;

        OnMaskAssigned?.Invoke(player, CurrentMask);
        AwardPotToPlayer(player);
        Debug.Log($"Player {player.PlayerIndex} {(isBlessed ? "BLESSED" : "")} mask assigned ({maskIndex}/{masks.Count})");

        if (autoDrawNextMask)
        {
            StopAllCoroutines();
            
            if (maskIndex < masks.Count)
            {
                StartCoroutine(DrawNextMaskAfterDelay());
            }
            else
            {
                Debug.Log("No more masks - auto-switching to tarot phase");
                StartCoroutine(DelayedTarotStart(maskDrawDelay));
            }
        }
    }

    IEnumerator DrawNextMaskAfterDelay()
    {
        yield return new WaitForSeconds(maskDrawDelay);
        
        if (maskIndex < masks.Count)
        {
            StartNextMask();
        }
        else
        {
            Debug.Log("No more masks to draw - phase complete");
        }
    }

    IEnumerator DelayedTarotStart(float delay)
    {
        yield return new WaitForSeconds(delay);
        BeginTarotPhase();
        DrawRandomTarot();
    }

    public void DrawRandomTarot()
    {
        // Check if we've drawn all tarots for this round
        if (tarotsDrawnThisRound >= tarotsPerRound)
        {
            Debug.Log($"All {tarotsPerRound} tarots drawn - bidding phase complete!");
            OnBiddingFinished?.Invoke(); // You'll need to add this event
            return;
        }
        
        if (tarotCards?.Count == 0)
        {
            Debug.LogError("No tarot cards!");
            return;
        }

        // Avoid repeating last tarot if possible
        int attempts = 0;
        do
        {
            CurrentTarot = tarotCards[UnityEngine.Random.Range(0, tarotCards.Count)];
            attempts++;
        } while (CurrentTarot == lastDrawnTarot && attempts < 10 && tarotCards.Count > 1);
        
        lastDrawnTarot = CurrentTarot;
        tarotsDrawnThisRound++; // Increment counter

        if (tarotSession == null)
        {
            tarotSession = new NormalBiddingSession(players, tarotBidIncrement);
            tarotSession.OnBidWinner += winner => 
            {
                OnTarotAssigned?.Invoke(winner, CurrentTarot);
                Debug.Log($"Tarot assigned to Player {winner.PlayerIndex} ({tarotsDrawnThisRound}/{tarotsPerRound})");
                
                // Auto-draw next tarot after delay
                if (IsTarotPhase)
                {
                    StartCoroutine(DrawNextTarotAfterDelay(1f));
                }
            };
            tarotSession.OnCurrentBidAmountChanged += amount => OnCurrentBidAmountChanged?.Invoke(amount);
        }
        else
        {
            tarotSession.ResetForNewTarot();
        }

        OnTarotDrawn?.Invoke(CurrentTarot);
        OnCurrentBidAmountChanged?.Invoke(tarotBidIncrement);
        StartTimer(tarotBidTimer);
        
        Debug.Log($"Drew tarot {tarotsDrawnThisRound}/{tarotsPerRound}: {CurrentTarot.name}");
    }


    IEnumerator DrawNextTarotAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        DrawRandomTarot();
    }

    public void PassOnCurrentItem(int playerIndex)
    {
        PlayerGold player = FindPlayerByIndex(playerIndex);
        if (player == null) return;

        if (IsTarotPhase && tarotSession != null)
        {
            // FIX: Stop timer when player passes
            isTimerRunning = false;
            
            tarotSession.PlayerPasses(player);
            OnPlayerKickedFromRound?.Invoke(player);
            Debug.Log($"Player {playerIndex} passed on tarot");
            
            // FIX: If tarot ends due to pass, draw next one automatically
            if (!tarotSession.HasActiveBidding()) // You'll need to add this method
            {
                StartCoroutine(DrawNextTarotAfterDelay(1f));
            }
        }
        else if (IsMaskPhase)
        {
            AssignMaskToPlayer(player);
            Debug.Log($"Player {playerIndex} passed and took mask");
        }
    }

    public void TakeMaskWithoutBid(int playerIndex)
    {
        PlayerGold player = FindPlayerByIndex(playerIndex);
        if (player != null && CurrentMask != null)
        {
            AssignMaskToPlayer(player);
            RestartTimer();
            Debug.Log($"Player {playerIndex} took mask without bidding");
        }
    }

    public void EndTarotBidding() => tarotSession?.EndBidding();

    void AwardPotToPlayer(PlayerGold player)
    {
        if (player == null) return;
        player.Add(currentPot);
        currentPot = startingPot;
        OnPotUpdated?.Invoke(currentPot);
    }

    // Utility methods
    public bool HasPlayerWonTarot(int playerIndex)
    {
        PlayerGold player = FindPlayerByIndex(playerIndex);
        return player != null && tarotSession?.HasPlayerWonTarot(player) == true;
    }

    public void SubmitBid(int playerIndex, int bidAmount)
    {
        PlayerGold player = FindPlayerByIndex(playerIndex);
        if (player != null) PlayerBid(player, bidAmount);
    }

    private PlayerGold FindPlayerByIndex(int playerIndex) => 
        players.Find(p => p.PlayerIndex == playerIndex);
}