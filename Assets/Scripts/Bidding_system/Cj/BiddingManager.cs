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

    [SerializeField] UIShowMask showMask;

    List<BiddingInputHandler> players = new List<BiddingInputHandler>();
    int turnIndex = 0;
    int maxTurnIndex;

    MaskObject currentMask;
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
                    yield return new WaitForSeconds(2f);
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


    }

    public void PlayGetMaskEffects()
    {

    }

    public void SetTurnContext(TurnContext turnContext)
    {
        lastTurn = turnContext;
    }

    void ShowMask()
    {
        currentMask = maskQueue.Dequeue();
        showMask.UpdateMask(currentMask);
    }
}
