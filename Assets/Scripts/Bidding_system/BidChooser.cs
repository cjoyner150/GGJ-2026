using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class BidChooser : MonoBehaviour
{
    [Header("Controller Reference")]
    public MaskBiddingController biddingController;

    [Header("UI References")]
    public RectTransform playerBidPanel;
    public TMP_Text playerIndexText;
    public TMP_Text currentBidText;
    public TMP_Text incrementText;
    public TMP_Text timerText;
    public TMP_Text potText;
    public Image playerColorIndicator;
    public GameObject phaseIndicator;

    [Header("Player Positions")]
    public RectTransform[] playerPositions;

    [Header("Bid Adjustment")]
    public Button increaseButton;
    public Button decreaseButton;
    public Button confirmButton;
    public int minBidIncrement = 10;

    [Header("Animation Settings")]
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
    public Transform potDisplayPosition;

    [Header("Runtime State")]
    public List<PlayerGold> players;
    public bool enableTestControls = true;

    private int currentBidAmount = 10;
    private int currentPlayerIndex = 0;
    private bool isActive = false;
    
    // Smooth movement variables
    private Coroutine panelMoveCoroutine;
    private Vector3 panelTargetPosition;
    private bool isPanelMoving = false;

    void Start()
    {
        // Setup UI
        increaseButton.onClick.AddListener(IncreaseBid);
        decreaseButton.onClick.AddListener(DecreaseBid);
        confirmButton.onClick.AddListener(ConfirmBid);

        // Subscribe to events if controller exists
        if (biddingController != null)
        {
            SubscribeToEvents();
        }
    }

    void Update()
    {
        if (!isActive) return;

        // Smooth panel movement update
        if (isPanelMoving)
        {
            playerBidPanel.position = Vector3.Lerp(
                playerBidPanel.position, 
                panelTargetPosition, 
                Time.deltaTime * (1f / panelMoveDuration)
            );
            
            // Check if close enough to target
            if (Vector3.Distance(playerBidPanel.position, panelTargetPosition) < 1f)
            {
                playerBidPanel.position = panelTargetPosition;
                isPanelMoving = false;
            }
        }
    }

    public void SubscribeToEvents()
    {
        if (biddingController == null) return;

        biddingController.OnPotUpdated += UpdatePotDisplay;
        biddingController.OnTimerUpdated += UpdateTimerDisplay;
        biddingController.OnMaskDrawn += OnMaskDrawn;
        biddingController.OnTarotDrawn += OnTarotDrawn;
        biddingController.OnMaskPhaseStarted += OnMaskPhaseStarted;
        biddingController.OnTarotPhaseStarted += OnTarotPhaseStarted;
        biddingController.OnPlayerBidPlaced += OnPlayerBidPlaced;
    }

    public void Initialize()
    {
        if (players == null || players.Count == 0)
        {
            Debug.LogError("No players assigned to BidChooser!");
            return;
        }

        currentPlayerIndex = 0;
        isActive = true;

        // Initial UI setup
        MovePanelToPlayer(currentPlayerIndex);
        UpdateCurrentBid();
        UpdateUI();

        Debug.Log($"BidChooser initialized with {players.Count} players");
    }

    // Smooth panel movement with lerp
    public void MovePanelToPlayer(int playerIndex)
    {
        if (playerPositions.Length == 0 || playerIndex >= playerPositions.Length)
            return;

        if (playerPositions[playerIndex] != null)
        {
            panelTargetPosition = playerPositions[playerIndex].position;
            isPanelMoving = true;
            
            // Stop any existing coroutine
            if (panelMoveCoroutine != null)
                StopCoroutine(panelMoveCoroutine);
            
            // Start smooth movement
            panelMoveCoroutine = StartCoroutine(SmoothMovePanel(panelTargetPosition));
        }

        if (playerIndex < players.Count)
        {
            PlayerGold player = players[playerIndex];
            playerIndexText.text = $"P{player.PlayerIndex + 1}";

            if (playerColorIndicator != null)
            {
                playerColorIndicator.color = player.PlayerColor;
            }
        }
    }
    
    IEnumerator SmoothMovePanel(Vector3 targetPosition)
    {
        Vector3 startPosition = playerBidPanel.position;
        float elapsed = 0f;
        
        while (elapsed < panelMoveDuration)
        {
            float t = elapsed / panelMoveDuration;
            // Smooth easing function
            t = t * t * (3f - 2f * t); // Smoothstep
            
            playerBidPanel.position = Vector3.Lerp(startPosition, targetPosition, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        playerBidPanel.position = targetPosition;
        isPanelMoving = false;
    }

    public void MoveToNextPlayer()
    {
        if (players == null || players.Count == 0) return;

        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        MovePanelToPlayer(currentPlayerIndex);
        UpdateCurrentBid();
        UpdateUI();
    }

    void UpdateCurrentBid()
    {
        if (biddingController == null) return;

        if (biddingController.IsMaskPhase)
            currentBidAmount = Mathf.Max(biddingController.maskBidCost, minBidIncrement);
        else if (biddingController.IsTarotPhase)
            currentBidAmount = Mathf.Max(biddingController.tarotBidIncrement, minBidIncrement);
    }

    void UpdateUI()
    {
        currentBidText.text = $"{currentBidAmount}G";

        if (biddingController != null)
        {
            string phase = biddingController.IsMaskPhase ? "MASK" : 
                          biddingController.IsTarotPhase ? "TAROT" : "IDLE";
            incrementText.text = $"{phase}: {currentBidAmount}G";

            // Update phase indicator
            if (phaseIndicator != null)
            {
                phaseIndicator.SetActive(true);
                TMP_Text phaseText = phaseIndicator.GetComponentInChildren<TMP_Text>();
                if (phaseText != null)
                {
                    phaseText.text = phase;
                    phaseText.color = biddingController.IsMaskPhase ? Color.red : Color.blue;
                }
            }
        }

        potText.text = $"{biddingController?.CurrentPot ?? 0}G";
        decreaseButton.interactable = currentBidAmount > minBidIncrement;
    }

    void UpdatePotDisplay(int potAmount)
    {
        int currentPot = int.Parse(potText.text.Replace("G", ""));
        StartCoroutine(AnimateNumberCounter(potText, currentPot, potAmount, 0.5f));
    }

    void UpdateTimerDisplay(float timeRemaining)
    {
        timerText.text = timeRemaining.ToString("F1");
        
        // Visual feedback
        if (timeRemaining < 3f)
        {
            timerText.color = Color.Lerp(Color.red, Color.yellow, timeRemaining / 3f);
        }
        else
        {
            timerText.color = Color.green;
        }
    }

    public void IncreaseBid()
    {
        currentBidAmount += minBidIncrement;
        UpdateUI();
    }

    public void DecreaseBid()
    {
        if (currentBidAmount > minBidIncrement)
        {
            currentBidAmount -= minBidIncrement;
            UpdateUI();
        }
    }

    public void ConfirmBid()
    {
        if (!isActive || players == null || currentPlayerIndex >= players.Count)
            return;

        PlayerGold currentPlayer = players[currentPlayerIndex];

        // Update controller's bid cost if we're bidding higher
        if (biddingController.IsMaskPhase && currentBidAmount > biddingController.maskBidCost)
        {
            biddingController.SetBidCost(currentBidAmount, true);
        }
        else if (biddingController.IsTarotPhase && currentBidAmount > biddingController.tarotBidIncrement)
        {
            biddingController.SetBidCost(currentBidAmount, false);
        }

        // Process the bid through controller
        biddingController.PlayerBid(currentPlayer);

        // Show floating text
        ShowFloatingText(currentBidAmount, currentPlayer.PlayerColor);

        // Move to next player
        MoveToNextPlayer();
    }

    void ShowFloatingText(int amount, Color color)
    {
        if (floatingTextPrefab == null) return;

        GameObject floatingText = Instantiate(floatingTextPrefab, playerBidPanel.position, Quaternion.identity, transform);
        TMP_Text textComponent = floatingText.GetComponent<TMP_Text>();

        if (textComponent != null)
        {
            textComponent.text = $"+{amount}";
            textComponent.color = color;
            StartCoroutine(AnimateFloatingText(floatingText.transform));
        }
    }

    IEnumerator AnimateFloatingText(Transform floatingText)
    {
        Vector3 startPos = floatingText.position;
        Vector3 endPos = potDisplayPosition != null ? potDisplayPosition.position : transform.position;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            float t = elapsed / animationDuration;
            
            // Smooth arc movement with lerp
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            // Add arc height (parabolic curve)
            float arcHeight = Mathf.Sin(t * Mathf.PI) * 100f;
            currentPos.y += arcHeight;
            
            floatingText.position = currentPos;

            // Scale animation
            float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.3f;
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

    IEnumerator PulseText(Transform target)
    {
        Vector3 originalScale = target.localScale;
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // Smooth lerp with bounce
            float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.3f;
            target.localScale = Vector3.Lerp(originalScale, originalScale * scale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localScale = originalScale;
    }

    // Event Handlers
    void OnMaskDrawn(Mask mask)
    {
        Debug.Log($"UI: Mask drawn - {mask.description}");
        
        // Animate mask display with lerp
        if (maskDisplayPrefab != null)
        {
            StartCoroutine(AnimateMaskCard(mask));
        }
    }

    void OnTarotDrawn(TarotCard tarot)
    {
        Debug.Log($"UI: Tarot drawn - {tarot.name}");
        
        // Animate tarot display with lerp
        if (tarotDisplayPrefab != null)
        {
            StartCoroutine(AnimateTarotCard(tarot));
        }
    }

    void OnMaskPhaseStarted()
    {
        Debug.Log("UI: Mask phase started");
        currentBidAmount = biddingController.maskBidCost;
        UpdateUI();
    }

    void OnTarotPhaseStarted()
    {
        Debug.Log("UI: Tarot phase started");
        currentBidAmount = biddingController.tarotBidIncrement;
        UpdateUI();
    }

    void OnPlayerBidPlaced(PlayerGold player)
    {
        Debug.Log($"UI: Player {player.PlayerIndex} placed a bid");
    }

    IEnumerator AnimateMaskCard(Mask mask)
    {
        if (maskDisplayPrefab == null || maskSpawnPoint == null) yield break;

        GameObject maskObj = Instantiate(maskDisplayPrefab, maskSpawnPoint.position, Quaternion.identity, transform);
        var display = maskObj.GetComponent<MaskDisplay>();
        if (display != null) display.SetMask(mask);

        yield return StartCoroutine(AnimateCardToTarget(maskObj.transform, maskTargetPoint.position));
        
        Destroy(maskObj, 2f);
    }

    IEnumerator AnimateTarotCard(TarotCard tarot)
    {
        if (tarotDisplayPrefab == null || tarotSpawnPoint == null) yield break;

        GameObject tarotObj = Instantiate(tarotDisplayPrefab, tarotSpawnPoint.position, Quaternion.identity, transform);
        var display = tarotObj.GetComponent<TarotDisplay>();
        if (display != null) display.SetTarot(tarot);

        yield return StartCoroutine(AnimateCardToTarget(tarotObj.transform, tarotTargetPoint.position));
        
        Destroy(tarotObj, 2f);
    }

    IEnumerator AnimateCardToTarget(Transform card, Vector3 targetPos)
    {
        Vector3 startPos = card.position;
        Quaternion startRot = card.rotation;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            float t = elapsed / animationDuration;
            
            // Smooth position lerp with bounce
            Vector3 position = Vector3.Lerp(startPos, targetPos, t);
            position.y += Mathf.Sin(t * Mathf.PI) * 50f;
            card.position = position;

            // Smooth rotation lerp with wobble
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
        
        // Final bounce effect
        elapsed = 0f;
        float bounceDuration = 0.2f;
        Vector3 finalScale = Vector3.one;
        
        while (elapsed < bounceDuration)
        {
            float t = elapsed / bounceDuration;
            float bounce = Mathf.Sin(t * Mathf.PI) * 0.1f;
            card.localScale = finalScale * (1f + bounce);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        card.localScale = finalScale;
    }

    // Animate number counter
    IEnumerator AnimateNumberCounter(TMP_Text textElement, int startValue, int endValue, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, t));
            textElement.text = $"{currentValue}G";
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        textElement.text = $"{endValue}G";
    }

    void OnDestroy()
    {
        // Clean up event subscriptions
        if (biddingController != null)
        {
            biddingController.OnPotUpdated -= UpdatePotDisplay;
            biddingController.OnTimerUpdated -= UpdateTimerDisplay;
            biddingController.OnMaskDrawn -= OnMaskDrawn;
            biddingController.OnTarotDrawn -= OnTarotDrawn;
            biddingController.OnMaskPhaseStarted -= OnMaskPhaseStarted;
            biddingController.OnTarotPhaseStarted -= OnTarotPhaseStarted;
            biddingController.OnPlayerBidPlaced -= OnPlayerBidPlaced;
        }
    }
}