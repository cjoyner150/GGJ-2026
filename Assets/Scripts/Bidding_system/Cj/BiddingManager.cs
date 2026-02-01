using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BiddingManager : MonoBehaviour
{
    [SerializeField] MaskSO goddessMaskSO;
    [SerializeField] MaskSO[] possibleMaskSOs;
    private Queue<MaskObject> maskQueue = new Queue<MaskObject>();

    [SerializeField] TarotSO[] possibleTarotSOs;
    private Queue<TarotObject> tarotQueue = new Queue<TarotObject>();

    [SerializeField] UIShowAuction showUI;

    List<BiddingInputHandler> players = new List<BiddingInputHandler>();
    int turnIndex = 0;
    int maxTurnIndex;

    MaskObject currentMask;
    TarotObject currentTarot;
    TurnContext lastTurn;

    WaitForSeconds maskEffectsTime = new WaitForSeconds(2f);
    WaitForSeconds delayForStartMaskBiddingTime = new WaitForSeconds(2f);
    WaitForSeconds delayBetweenAuctionsTime = new WaitForSeconds(3f);

    public void Initialize(List<BiddingInputHandler> players)
    {
        this.players = players;

        List<MaskSO> maskSOs = possibleMaskSOs.ToList();

        for (int i = 0; i < PlayerConfigManager.Instance.GetPlayerConfigs().Count - 1; i++)
        {
            MaskSO so = maskSOs[Random.Range(0, maskSOs.Count)];

            maskQueue.Enqueue(new MaskObject(so));
            maskSOs.Remove(so);
        }

        maskQueue.Enqueue(new MaskObject(goddessMaskSO));

        List<TarotSO> tarotSOs = possibleTarotSOs.ToList();

        for (int i = 0; i < PlayerConfigManager.Instance.GetPlayerConfigs().Count; i++)
        {
            for (int j = 0; j < 2; j++)
            { 
                TarotSO so = tarotSOs[Random.Range(0, tarotSOs.Count)];
                tarotQueue.Enqueue(new TarotObject(so));
                tarotSOs.Remove(so);
            }
        }

        maxTurnIndex = players.Count - 1;
        turnIndex = 0;
    }

    private void Start()
    {
        StartGame();
    }

    private void StartGame()
    {
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        List<BiddingInputHandler> currentPlayers = players.ToArray().ToList();

        PlayMaskStartEffects();

        yield return delayForStartMaskBiddingTime;

        while (true) // Mask loop
        {
            maxTurnIndex = currentPlayers.Count - 1;
            turnIndex = 0;
            int requiredAmount = 10;

            ShowMask();

            int pot = 0;

            while (true) // Bidding on mask
            {
                BiddingInputHandler player = currentPlayers[turnIndex];
                player.OnTurnEnter(requiredAmount);

                turnIndex++;
                if (turnIndex > maxTurnIndex) turnIndex = 0;

                while (player.IsTurn)
                {
                    yield return null;
                }

                if (lastTurn.passedTurn) {
                    lastTurn.player.cfg.Acorns += pot;
                    lastTurn.player.cfg.Mask = currentMask;
                    currentPlayers.Remove(lastTurn.player);

                    PlayGetMaskEffects();
                    yield return maskEffectsTime;
                    break;
                }

                lastTurn.player.cfg.Acorns -= lastTurn.acornBidAmount;
                pot += lastTurn.acornBidAmount;
                requiredAmount = lastTurn.acornBidAmount;
                yield return null;
            }

            if (currentPlayers.Count < 2)
            {
                break;
            }

            yield return null;

        }

        PlayAuctionTransitionEffects();
        yield return delayBetweenAuctionsTime;

        currentPlayers = players.ToArray().ToList();

        PlayTarotStartEffects();
        yield return delayForStartMaskBiddingTime;

        while (true) // Tarot loop
        {
            maxTurnIndex = currentPlayers.Count - 1;
            turnIndex = 0;
            int requiredAmount = 10;

            ShowTarot();

            while (true) // Bidding on tarot
            {
                BiddingInputHandler player = currentPlayers[turnIndex];
                player.OnTurnEnter(requiredAmount);

                turnIndex++;
                if (turnIndex > maxTurnIndex) turnIndex = 0;

                while (player.IsTurn)
                {
                    yield return null;
                }

                if (lastTurn.passedTurn)
                {
                    currentPlayers.Remove(lastTurn.player);
                }

                lastTurn.player.cfg.Acorns -= lastTurn.acornBidAmount;
                requiredAmount = lastTurn.acornBidAmount;

                if (currentPlayers.Count < 2)
                {
                    lastTurn.player.cfg.Tarots.Add(currentTarot);
                    break;
                }

                yield return null;
            }

            if (currentPlayers.Count < 2)
            {
                break;
            }

            yield return null;

        }
    }


    public void PlayGetMaskEffects()
    {

    }

    public void PlayMaskStartEffects()
    {

    }

    public void PlayAuctionTransitionEffects()
    {

    }

    public void PlayTarotStartEffects()
    {

    }

    public void PlayGetTarotEffects()
    {

    }

    public void SetTurnContext(TurnContext turnContext)
    {
        lastTurn = turnContext;
    }

    private void ShowTarot()
    {
        currentTarot = tarotQueue.Dequeue();
        showUI.UpdateTarot(currentTarot);
    }

    void ShowMask()
    {
        currentMask = maskQueue.Dequeue();
        showUI.UpdateMask(currentMask);
    }
}
