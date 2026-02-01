
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class BidChooser : MonoBehaviour
{
    [Header("Controller")]
    public MaskBiddingController controller;

    [Header("UI Elements")]
    public RectTransform playerBidPanel;
    public TMP_Text playerIndexText;
    public TMP_Text currentBidText;
    public TMP_Text timerText;
    public TMP_Text potText;
    public TMP_Text minimumBidText;
    public Image playerColorIndicator;
    public GameObject phaseIndicator;

    [Header("Player Positions")]
    public RectTransform[] playerPositions;

    [Header("Controls")]
    public Button increaseButton;
    public Button decreaseButton;
    public Button confirmButton;
    public Button passButton;
    public int minBidIncrement = 10;

    [Header("Animation")]
    public GameObject maskDisplayPrefab;
    public GameObject tarotDisplayPrefab;
    public Transform maskSpawnPoint;
    public Transform maskTargetPoint;
    public Transform tarotSpawnPoint;
    public Transform tarotTargetPoint;
    public float animationDuration = 1f;
    public float panelMoveDuration = 0.5f;

    [Header("Floating Text")]
    public GameObject floatingTextPrefab;
    public GameObject kickedTextPrefab;
    public Transform potDisplayPosition;

    [Header("Runtime")]
    public List<PlayerGold> players;

    private int currentBidAmount;
    private int currentMinimumBid;
    private int currentPlayerIndex = 0;
    private bool isActive = false;
    private Vector3 panelTargetPos;
    private bool isPanelMoving = false;
    
    // Player tracking
    private HashSet<int> playersWithMasks = new HashSet<int>();
    private HashSet<int> playersWithTarots = new HashSet<int>();
    private HashSet<int> kickedPlayers = new HashSet<int>();

    void Start()
    {
        SetupButtons();
        
        // Try to find controller if not set
        if (controller == null)
        {
            controller = FindObjectOfType<MaskBiddingController>();
            Debug.Log($"Found controller: {controller != null}");
        }
        
        // Try to find players if not set
        if (players == null || players.Count == 0)
        {
            PlayerGold[] allPlayers = FindObjectsOfType<PlayerGold>();
            players = new List<PlayerGold>(allPlayers);
            Debug.Log($"Found {players.Count} players");
        }
        
        if (controller != null)
        {
            SubscribeToEvents();
            if (controller.IsMaskPhase)
            {
                isActive = true;
                OnMaskPhaseStarted();
            }
            else if (controller.IsTarotPhase)
            {
                isActive = true;
                OnTarotPhaseStarted();
            }
        }
        else
        {
            Debug.LogError("No MaskBiddingController found! Please assign one in the inspector.");
        }
    }

    IEnumerator DelayedStart(float delay)
    {
        yield return new WaitForSeconds(delay);
        controller.BeginMaskPhase();
    }

    void Update()
    {
        if (!isActive) return;

        if (isPanelMoving)
        {
            playerBidPanel.position = Vector3.Lerp(
                playerBidPanel.position, 
                panelTargetPos, 
                Time.deltaTime / panelMoveDuration
            );
            
            if (Vector3.Distance(playerBidPanel.position, panelTargetPos) < 1f)
            {
                playerBidPanel.position = panelTargetPos;
                isPanelMoving = false;
            }
        }
        
        // Smooth font size back to normal
        if (currentBidText.fontSize > 32)
            currentBidText.fontSize = Mathf.Lerp(currentBidText.fontSize, 32, Time.deltaTime * 5f);
    }

    void SetupButtons()
    {
        increaseButton.onClick.AddListener(() => AdjustBid(minBidIncrement));
        decreaseButton.onClick.AddListener(() => AdjustBid(-minBidIncrement));
        confirmButton.onClick.AddListener(ConfirmBid);
        
        if (passButton != null)
            passButton.onClick.AddListener(PassBid);
        else
            Debug.LogWarning("Pass button not assigned!");
    }

    void PassBid()
    {
        if (!isActive) return;
        
        PlayerGold player = GetCurrentPlayer();
        if (player == null)
        {
            Debug.LogError("No current player to pass!");
            return;
        }

        Debug.Log($"=== PASS BID ===");
        Debug.Log($"Player {player.PlayerIndex} passing in {(controller.IsMaskPhase ? "MASK" : "TAROT")} phase");

        if (controller == null)
        {
            Debug.LogError("No bidding controller!");
            return;
        }

        if (controller.IsMaskPhase)
        {
            Debug.Log($"Player {player.PlayerIndex} passed - taking mask!");
            ShowFloatingText("PASS + MASK", Color.red);
            controller.TakeMaskWithoutBid(player.PlayerIndex);
            
        }
        else if (controller.IsTarotPhase)
        {
            Debug.Log($"Player {player.PlayerIndex} passed on tarot");
            ShowFloatingText("PASSED", Color.gray);
            controller.PassOnCurrentItem(player.PlayerIndex);
            
            // Track as kicked from current tarot
            if (!kickedPlayers.Contains(player.PlayerIndex))
            {
                kickedPlayers.Add(player.PlayerIndex);
                ShowFloatingText($"P{player.PlayerIndex + 1} OUT", Color.red);
            }
            
            StartCoroutine(DelayedMoveToNextPlayer(0.5f));
        }
        else
        {
            Debug.LogWarning("Pass called but no active phase!");
            return;
        }
    }

    IEnumerator DelayedMoveToNextPlayer(float delay)
    {
        yield return new WaitForSeconds(delay);
        MoveToNextPlayer();
    }

    void SubscribeToEvents()
    {
        if (controller == null) return;
        
        Debug.Log("Subscribing to controller events");
        
        controller.OnPotUpdated += UpdatePotDisplay;
        controller.OnTimerUpdated += UpdateTimerDisplay;
        controller.OnMaskDrawn += OnMaskDrawn;
        controller.OnTarotDrawn += OnTarotDrawn;
        controller.OnMaskPhaseStarted += OnMaskPhaseStarted;
        controller.OnTarotPhaseStarted += OnTarotPhaseStarted;
        controller.OnPlayerBidPlaced += OnPlayerBidPlaced;
        controller.OnPlayerKickedFromRound += OnPlayerKickedFromRound;
        controller.OnMaskAssigned += OnMaskAssigned;
        controller.OnCurrentBidAmountChanged += OnCurrentBidAmountChanged;
        controller.OnTarotAssigned += OnTarotAssigned;
        controller.OnBiddingFinished += OnBiddingFinished;
    }

    void UpdatePotDisplay(int potAmount)
    {
        if (potText) 
        {
            potText.text = $"Pot: {potAmount}G";
            Debug.Log($"Pot updated: {potAmount}G");
        }
    }
    void OnBiddingFinished()
    {
        Debug.Log("BidChooser: Bidding finished!");
        isActive = false;
        ShowFloatingText("BIDDING COMPLETE!", Color.green, true);
    }

    void UpdateTimerDisplay(float time)
    {
        if (!timerText) return;
        
        timerText.text = time.ToString("F1") + "s";
        
        // Color feedback
        if (time < 3f)
            timerText.color = Color.Lerp(Color.red, Color.yellow, time / 3f);
        else
            timerText.color = Color.green;
    }

    void OnMaskDrawn(Mask mask)
    {
        Debug.Log($"BidChooser: New mask drawn - {mask.description}");
        
        // Reset bid to minimum for new mask
        currentMinimumBid = controller.maskBidCost;
        currentBidAmount = currentMinimumBid;
        
        // Start with first valid player
        if (ShouldSkipPlayer(currentPlayerIndex))
            MoveToNextPlayer();
        else
            UpdateUI();
        
        // Animate mask display
        if (maskDisplayPrefab)
            StartCoroutine(AnimateMaskCard(mask));
    }

    void OnTarotDrawn(TarotCard tarot)
    {
        Debug.Log($"BidChooser: New tarot drawn - {tarot.name}");
        
        ResetForNewTarot();  // <-- USE THE METHOD
        
        // Animate tarot display
        if (tarotDisplayPrefab)
            StartCoroutine(AnimateTarotCard(tarot));
    }

    void OnTarotPhaseStarted()
    {
        Debug.Log("BidChooser: Tarot phase started");
        
        playersWithTarots.Clear();
        ResetForNewTarot();  // <-- USE THE METHOD
        isActive = true;
        
        UpdateUI();
        UpdatePhaseIndicator();
    }

    void OnPlayerBidPlaced(PlayerGold player)
    {
        Debug.Log($"BidChooser: Player {player.PlayerIndex} placed bid");
        
        // Auto-move to next player after bid
        StartCoroutine(DelayedMoveToNextPlayer(0.3f));
    }

    void OnPlayerKickedFromRound(PlayerGold player)
    {
        Debug.Log($"BidChooser: Player {player.PlayerIndex} kicked from round");
        
        ShowKickedNotification(player);
    }

    void OnMaskAssigned(PlayerGold player, Mask mask)
    {
        Debug.Log($"BidChooser: Mask assigned to Player {player.PlayerIndex}");
        OnItemAssigned(player, true);
    }

    void OnTarotAssigned(PlayerGold player, TarotCard tarot)
    {
        Debug.Log($"BidChooser: Tarot assigned to Player {player.PlayerIndex}");
        
        OnItemAssigned(player, false);
    }

    void OnCurrentBidAmountChanged(int amount)
    {
        Debug.Log($"BidChooser: Minimum bid changed to {amount}G");
        currentMinimumBid = amount;
        currentBidAmount = Mathf.Max(currentBidAmount, amount);
        UpdateUI();
    }

    public void Initialize(int baseBid)
    {
        currentPlayerIndex = 0;
        currentMinimumBid = baseBid;
        currentBidAmount = baseBid;
        isActive = true;
        MovePanelToPlayer(0);
        UpdateUI();
        UpdatePhaseIndicator();
    }

    public void Initialize()
    {
        currentPlayerIndex = 0;
        isActive = false;
    }

    public void SetCurrentBid(int amount)
    {
        currentBidAmount = Mathf.Max(currentMinimumBid, amount);
        UpdateUI();
    }

    void MovePanelToPlayer(int index)
    {
        if (index < 0 || index >= players.Count)
        {
            Debug.LogError($"Invalid player index: {index}");
            return;
        }

        if (playerPositions.Length > index && playerPositions[index] != null)
        {
            panelTargetPos = playerPositions[index].position;
            isPanelMoving = true;
        }

        PlayerGold player = players[index];
        playerIndexText.text = $"P{player.PlayerIndex + 1}";
        if (playerColorIndicator) 
            playerColorIndicator.color = player.PlayerColor;

        currentPlayerIndex = index;
        Debug.Log($"Moved panel to Player {currentPlayerIndex}");
        
        UpdateUI();
    }

    void AdjustBid(int delta)
    {
        int newBid = currentBidAmount + delta;
        if (newBid >= currentMinimumBid)
        {
            currentBidAmount = newBid;
            UpdateUI();
            
            // Visual feedback
            currentBidText.fontSize = 40;
            
            // Show floating text
            if (floatingTextPrefab)
            {
                string text = delta > 0 ? $"+{delta}G" : $"{delta}G";
                Color color = delta > 0 ? Color.green : Color.red;
                ShowFloatingText(text, color, false);
            }
        }
        else
        {
            Debug.Log($"Cannot decrease bid below minimum {currentMinimumBid}");
        }
    }

    void ConfirmBid()
    {
        if (!isActive) 
        {
            Debug.LogWarning("BidChooser not active!");
            return;
        }
        
        PlayerGold player = GetCurrentPlayer();
        if (player == null)
        {
            Debug.LogError("No current player!");
            return;
        }

        Debug.Log($"=== CONFIRM BID ===");
        Debug.Log($"Player {player.PlayerIndex} attempting bid: {currentBidAmount}G (minimum: {currentMinimumBid}G)");
        Debug.Log($"Player gold: {player.Gold}G");

        // Validate bid
        if (!player.CanAfford(currentBidAmount))
        {
            Debug.LogError($"Player {player.PlayerIndex} cannot afford {currentBidAmount}!");
            ShowErrorText("Not enough gold!", Color.red);
            return;
        }

        if (currentBidAmount < currentMinimumBid)
        {
            Debug.LogError($"Bid {currentBidAmount} is below minimum {currentMinimumBid}!");
            ShowErrorText($"Must bid at least {currentMinimumBid}G!", Color.red);
            return;
        }

        // Submit bid through controller
        if (controller != null)
        {
            controller.PlayerBid(player, currentBidAmount);
            Debug.Log($"Bid submitted for Player {player.PlayerIndex}: {currentBidAmount}G");
            
            // Visual feedback for raise
            if (currentBidAmount > currentMinimumBid)
            {
                Debug.Log($"Player {player.PlayerIndex} RAISED to {currentBidAmount}G!");
                ShowRaiseNotification(currentBidAmount, player.PlayerColor);
                currentMinimumBid = currentBidAmount; // Update minimum for next players
            }
            
            ShowFloatingText($"{currentBidAmount}G", player.PlayerColor, currentBidAmount > currentMinimumBid);
        }
        else
        {
            Debug.LogError("Controller is null!");
        }
    }

    void ShowRaiseNotification(int amount, Color color)
    {
        if (!floatingTextPrefab) return;

        GameObject raiseText = Instantiate(floatingTextPrefab, playerBidPanel.position, Quaternion.identity, transform);
        if (raiseText.TryGetComponent<TMP_Text>(out var text))
        {
            text.text = $"RAISE!\nNew min: {amount}G";
            text.color = Color.yellow;
            text.fontSize = 40;
            StartCoroutine(AnimateAndDestroy(raiseText.transform, 1.5f));
        }
    }

    void MoveToNextPlayer()
    {
        if (players.Count == 0)
        {
            Debug.LogError("No players in list!");
            return;
        }

        int originalIndex = currentPlayerIndex;
        int attempts = 0;
        int maxAttempts = players.Count * 2;
        
        do
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            attempts++;
            
            if (attempts > maxAttempts)
            {
                Debug.LogError("Couldn't find valid player! All players might be skipped.");
                currentPlayerIndex = originalIndex;
                break;
            }
        } 
        while (ShouldSkipPlayer(currentPlayerIndex));
        
        // Only move if we found a different player
        if (currentPlayerIndex != originalIndex)
        {
            MovePanelToPlayer(currentPlayerIndex);
            currentBidAmount = currentMinimumBid;
            UpdateUI();
            
            Debug.Log($"Moved from Player {originalIndex} to Player {currentPlayerIndex}, bid reset to {currentMinimumBid}G");
        }
        else
        {
            Debug.LogWarning("No valid players found to move to!");
        }
    }

    bool ShouldSkipPlayer(int index)
    {
        if (index >= players.Count) return true;
        
        PlayerGold player = players[index];
        if (player == null) return true;
        
        // Can't afford current minimum bid
        if (!player.CanAfford(currentMinimumBid))
        {
            Debug.Log($"Player {player.PlayerIndex} can't afford {currentMinimumBid}G (has {player.Gold}G)");
            
            // In mask phase, auto-assign mask to bankrupt player
            if (controller.IsMaskPhase && !playersWithMasks.Contains(player.PlayerIndex))
            {
                Debug.Log($"Player {player.PlayerIndex} bankrupt - auto-assigning mask!");
                controller.TakeMaskWithoutBid(player.PlayerIndex);
            }
            return true;
        }
        
        // Mask phase: skip if already has ANY mask from this phase
        if (controller.IsMaskPhase && playersWithMasks.Contains(player.PlayerIndex))
        {
            Debug.Log($"Player {player.PlayerIndex} already has mask from this phase - skipping");
            return true;
        }
        
        // Tarot phase: skip if already won a tarot in THIS ROUND
        if (controller.IsTarotPhase && playersWithTarots.Contains(player.PlayerIndex))
        {
            Debug.Log($"Player {player.PlayerIndex} already won tarot this round - skipping");
            return true;
        }
        
        // Tarot phase: skip if kicked from CURRENT tarot only
        if (controller.IsTarotPhase && kickedPlayers.Contains(player.PlayerIndex))
        {
            Debug.Log($"Player {player.PlayerIndex} kicked from current tarot - skipping");
            return true;
        }
        
        return false;
    }

    void UpdateUI()
    {
        PlayerGold player = GetCurrentPlayer();
        if (player == null) 
        {
            Debug.LogWarning("No current player for UI update");
            return;
        }

        Debug.Log($"Updating UI for Player {player.PlayerIndex}: Bid={currentBidAmount}G, Min={currentMinimumBid}G, Gold={player.Gold}G");

        // Update bid displays
        currentBidText.text = $"{currentBidAmount}G";
        minimumBidText.text = $"Min: {currentMinimumBid}G";
        
        // Color code current bid
        if (currentBidAmount > currentMinimumBid)
            currentBidText.color = Color.yellow;
        else if (player.CanAfford(currentBidAmount))
            currentBidText.color = Color.white;
        else
            currentBidText.color = Color.red;
        
        // Update minimum bid text color
        minimumBidText.color = currentBidAmount > currentMinimumBid ? Color.yellow : Color.white;
        
        // Update pot display
        if (potText && controller != null) 
            potText.text = $"Pot: {controller.CurrentPot}G";
        
        // Enable/disable buttons
        decreaseButton.interactable = currentBidAmount > currentMinimumBid;
        confirmButton.interactable = player.CanAfford(currentBidAmount);
        
        if (passButton != null)
            passButton.interactable = player.CanAfford(currentMinimumBid);
        
        UpdatePhaseIndicator();
    }
    void OnMaskPhaseStarted()
    {
        Debug.Log("BidChooser: Mask phase started");
        
        // Clear all tracking
        playersWithMasks.Clear();
        playersWithTarots.Clear();
        kickedPlayers.Clear();
        
        // Reset to Player 0 for first mask
        currentPlayerIndex = 0;
        currentMinimumBid = controller.maskBidCost;
        currentBidAmount = currentMinimumBid;
        isActive = true;
        
        // Start with first valid player
        if (ShouldSkipPlayer(currentPlayerIndex))
            MoveToNextPlayer();
        else
            MovePanelToPlayer(currentPlayerIndex);
        
        UpdateUI();
        UpdatePhaseIndicator();
    }

    void UpdatePhaseIndicator()
    {
        if (phaseIndicator == null || controller == null) return;
        
        phaseIndicator.SetActive(true);
        TMP_Text phaseText = phaseIndicator.GetComponentInChildren<TMP_Text>();
        if (phaseText != null)
        {
            string phase = controller.IsMaskPhase ? "MASK" : 
                          controller.IsTarotPhase ? "TAROT" : "IDLE";
            phaseText.text = phase;
            phaseText.color = controller.IsMaskPhase ? Color.red : Color.blue;
        }
    }

    PlayerGold GetCurrentPlayer() => 
        currentPlayerIndex < players.Count ? players[currentPlayerIndex] : null;

    void OnItemAssigned(PlayerGold player, bool isMask)
    {
        Debug.Log($"BidChooser: Item assigned to Player {player.PlayerIndex} (isMask: {isMask})");
        
        if (isMask)
        {
            // Track mask assignment
            if (!playersWithMasks.Contains(player.PlayerIndex))
            {
                playersWithMasks.Add(player.PlayerIndex);
                Debug.Log($"Player {player.PlayerIndex} received mask - OUT for remaining masks");
            }
        }
        else
        {
            if (!playersWithTarots.Contains(player.PlayerIndex))
            {
                playersWithTarots.Add(player.PlayerIndex);
            }
            
            Debug.Log($"Tarot assigned - kickedPlayers will reset on next tarot draw");
        }
        
        string itemType = isMask ? "mask" : "tarot";
        ShowFloatingText($"P{player.PlayerIndex + 1} gets {itemType}!", player.PlayerColor, false);
        
        PlayerGold currentPlayer = GetCurrentPlayer();
        if (currentPlayer != null && currentPlayer.PlayerIndex == player.PlayerIndex)
        {
            Debug.Log($"Current player got {itemType} - moving to next player");
            StartCoroutine(DelayedMoveToNextPlayer(0.3f));
        }
        else
        {
            currentBidAmount = currentMinimumBid;
            UpdateUI();
        }
    }

    void ShowKickedNotification(PlayerGold player)
    {
        // Track that this player was kicked from current tarot
        if (controller != null && controller.IsTarotPhase && !kickedPlayers.Contains(player.PlayerIndex))
        {
            kickedPlayers.Add(player.PlayerIndex);
            Debug.Log($"BidChooser: Tracked Player {player.PlayerIndex} kicked from current tarot");
        }
        
        GameObject prefab = kickedTextPrefab ?? floatingTextPrefab;
        if (!prefab) return;

        GameObject obj = Instantiate(prefab, playerBidPanel.position, Quaternion.identity, transform);
        if (obj.TryGetComponent<TMP_Text>(out var text))
        {
            text.text = $"P{player.PlayerIndex + 1} KICKED!";
            text.color = Color.red;
            text.fontSize = 48;
            StartCoroutine(AnimateKickedText(obj.transform));
        }
    }

    void ShowFloatingText(string message, Color color, bool isRaising = false)
    {
        if (!floatingTextPrefab) return;

        GameObject obj = Instantiate(floatingTextPrefab, playerBidPanel.position, 
                                    Quaternion.identity, transform);
        if (obj.TryGetComponent<TMP_Text>(out var text))
        {
            text.text = message;
            text.color = color;
            text.fontSize = isRaising ? 48 : 36;
            StartCoroutine(AnimateFloatingText(obj.transform, isRaising));
        }
    }

    void ShowErrorText(string message, Color color) => ShowFloatingText(message, color, false);

    IEnumerator AnimateFloatingText(Transform floatingText, bool isRaising)
    {
        Vector3 startPos = floatingText.position;
        Vector3 endPos = potDisplayPosition != null ? potDisplayPosition.position : transform.position;
        float elapsed = 0f;
        float duration = isRaising ? animationDuration * 1.5f : animationDuration;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            
            // Smooth arc movement
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            float arcHeight = Mathf.Sin(t * Mathf.PI) * (isRaising ? 150f : 100f);
            currentPos.y += arcHeight;
            floatingText.position = currentPos;

            // Scale animation
            float maxScale = isRaising ? 1.5f : 1.3f;
            float scale = 1f + Mathf.Sin(t * Mathf.PI) * (maxScale - 1f);
            floatingText.localScale = Vector3.one * scale;

            // Fade out
            var text = floatingText.GetComponent<TMP_Text>();
            if (text != null)
            {
                Color c = text.color;
                c.a = 1f - t;
                text.color = c;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(floatingText.gameObject);
    }
    void ResetForNewTarot()
    {
        Debug.Log("=== RESET FOR NEW TAROT ===");
        Debug.Log($"Before: kickedPlayers has {kickedPlayers.Count} players");
        
        kickedPlayers.Clear();
        currentPlayerIndex = 0;
        currentMinimumBid = controller.tarotBidIncrement;
        currentBidAmount = currentMinimumBid;
        
        Debug.Log($"After: kickedPlayers cleared");
        
        // Move to first valid player
        if (ShouldSkipPlayer(currentPlayerIndex))
            MoveToNextPlayer();
        else
            MovePanelToPlayer(currentPlayerIndex);
    }

    IEnumerator AnimateKickedText(Transform kickedText)
    {
        Vector3 startPos = kickedText.position;
        float elapsed = 0f;
        float duration = 2f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            
            // Shake effect
            float shake = Mathf.Sin(t * 20f) * 10f * (1f - t);
            Vector3 offset = new Vector3(shake, 0, 0);
            kickedText.position = startPos + offset;

            // Scale pulse
            float scale = 1f + Mathf.Sin(t * Mathf.PI * 4f) * 0.2f;
            kickedText.localScale = Vector3.one * scale;

            // Fade out
            var text = kickedText.GetComponent<TMP_Text>();
            if (text != null)
            {
                Color c = text.color;
                c.a = 1f - t;
                text.color = c;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(kickedText.gameObject);
    }

    IEnumerator AnimateAndDestroy(Transform obj, float duration, bool shake = false)
    {
        Vector3 startPos = obj.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            
            if (shake)
            {
                float shakeAmount = Mathf.Sin(t * 20f) * 10f * (1f - t);
                obj.position = startPos + new Vector3(shakeAmount, 0, 0);
            }
            else
            {
                obj.position = startPos + Vector3.up * (t * 50f);
            }

            // Fade out
            if (obj.TryGetComponent<TMP_Text>(out var text))
            {
                Color c = text.color;
                c.a = 1f - t;
                text.color = c;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(obj.gameObject);
    }

    IEnumerator AnimateMaskCard(Mask mask)
    {
        if (!maskDisplayPrefab || !maskSpawnPoint) yield break;

        GameObject obj = Instantiate(maskDisplayPrefab, maskSpawnPoint.position, Quaternion.identity, transform);
        if (obj.TryGetComponent<MaskDisplay>(out var display)) display.SetMask(mask);

        yield return AnimateCardToTarget(obj.transform, maskTargetPoint.position);
        Destroy(obj, 2f);
    }

    IEnumerator AnimateTarotCard(TarotCard tarot)
    {
        if (!tarotDisplayPrefab || !tarotSpawnPoint) yield break;

        GameObject obj = Instantiate(tarotDisplayPrefab, tarotSpawnPoint.position, Quaternion.identity, transform);
        if (obj.TryGetComponent<TarotDisplay>(out var display)) display.SetTarot(tarot);

        yield return AnimateCardToTarget(obj.transform, tarotTargetPoint.position);
        Destroy(obj, 2f);
    }

    IEnumerator AnimateCardToTarget(Transform card, Vector3 targetPos)
    {
        Vector3 startPos = card.position;
        Quaternion startRot = card.rotation;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            float t = elapsed / animationDuration;
            
            // Smooth position with bounce
            Vector3 position = Vector3.Lerp(startPos, targetPos, t);
            position.y += Mathf.Sin(t * Mathf.PI) * 50f;
            card.position = position;

            // Smooth rotation with wobble
            float wobble = Mathf.Sin(t * Mathf.PI * 2f) * 5f;
            Quaternion targetRotation = Quaternion.Euler(0, 0, wobble);
            card.rotation = Quaternion.Slerp(startRot, targetRotation, t);

            // Smooth scale animation
            float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.2f;
            card.localScale = Vector3.Lerp(Vector3.one, Vector3.one * scale, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        card.position = targetPos;
        card.rotation = Quaternion.identity;
        card.localScale = Vector3.one;
    }

    void OnDestroy()
    {
        if (controller != null)
        {
            controller.OnPotUpdated -= UpdatePotDisplay;
            controller.OnTimerUpdated -= UpdateTimerDisplay;
            controller.OnMaskDrawn -= OnMaskDrawn;
            controller.OnTarotDrawn -= OnTarotDrawn;
            controller.OnMaskPhaseStarted -= OnMaskPhaseStarted;
            controller.OnTarotPhaseStarted -= OnTarotPhaseStarted;
            controller.OnPlayerBidPlaced -= OnPlayerBidPlaced;
            controller.OnPlayerKickedFromRound -= OnPlayerKickedFromRound;
            controller.OnMaskAssigned -= OnMaskAssigned;
            controller.OnCurrentBidAmountChanged -= OnCurrentBidAmountChanged;
            controller.OnTarotAssigned -= OnTarotAssigned;
        }
    }

    // Public methods for debugging
    public void DebugStartMaskPhase()
    {
        if (controller != null)
        {
            controller.BeginMaskPhase();
            Debug.Log("Debug: Started mask phase");
        }
    }

    public void DebugStartTarotPhase()
    {
        if (controller != null)
        {
            controller.BeginTarotPhase();
            Debug.Log("Debug: Started tarot phase");
        }
    }

    public void DebugDrawTarot()
    {
        if (controller != null)
        {
            controller.DrawRandomTarot();
            Debug.Log("Debug: Drew random tarot");
        }
    }

    public void DebugForceNextPlayer()
    {
        MoveToNextPlayer();
        Debug.Log($"Debug: Forced move to Player {currentPlayerIndex}");
    }
}
