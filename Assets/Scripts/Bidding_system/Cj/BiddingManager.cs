using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    WaitForSeconds delayForStartMaskBiddingTime = new WaitForSeconds(.5f);
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

            if (currentPlayers.Count < 2)
            {
                currentPlayers[0].cfg.Mask = currentMask;
                PlayGetMaskEffects();
                yield return maskEffectsTime;
                break;
            }

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

                if (lastTurn.passedTurn)
                {
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

            yield return null;

        }

        foreach (var player in players)
        {
            print(player.cfg.PlayerIndex + " " + player.cfg.Mask.name);
        }

        PlayAuctionTransitionEffects();
        yield return delayBetweenAuctionsTime;

        currentPlayers = players.ToList();

        PlayTarotStartEffects();
        yield return delayForStartMaskBiddingTime;

        while (true) // Tarot loop
        {
            List<BiddingInputHandler> playersInBid = currentPlayers.ToList();

            maxTurnIndex = currentPlayers.Count - 1;
            turnIndex = 0;
            int requiredAmount = 10;

            ShowTarot();

            while (true) // Bidding on tarot
            {
                BiddingInputHandler player = playersInBid[turnIndex];

                if (player.cfg.Tarots == null) player.cfg.Tarots = new List<TarotObject>();

                player.OnTurnEnter(requiredAmount);

                turnIndex++;
                if (turnIndex > maxTurnIndex) turnIndex = 0;

                while (player.IsTurn)
                {
                    yield return null;
                }

                if (lastTurn.passedTurn)
                {
                    playersInBid.Remove(lastTurn.player);
                    turnIndex--;
                    maxTurnIndex--;
                }

                lastTurn.player.cfg.Acorns -= lastTurn.acornBidAmount;
                requiredAmount = lastTurn.acornBidAmount;

                if (playersInBid.Count < 2)
                {
                    lastTurn.player.cfg.Tarots.Add(currentTarot);
                    if (lastTurn.player.cfg.Tarots.Count == 2)
                    {
                        currentPlayers.Remove(lastTurn.player);
                    }
                    PlayGetTarotEffects();
                    yield return maskEffectsTime;
                    break;
                }

                yield return null;
            }

            if (currentPlayers.Count < 2)
            {
                while (tarotQueue.Count > 0)
                {

                    if (currentPlayers[0].cfg.Tarots == null) currentPlayers[0].cfg.Tarots = new List<TarotObject>();
                    currentPlayers[0].cfg.Tarots.Add(tarotQueue.Dequeue());
                    PlayGetTarotEffects();
                    yield return maskEffectsTime;
                }

                break;
            }

            yield return null;

        }

        PlayEndAuctionEffects();
        SceneManager.LoadScene(2);
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

    public void PlayEndAuctionEffects()
    {
        foreach (var player in players)
        {
            string tarotString = "";

            foreach (var tarot in player.cfg.Tarots)
            {
                tarotString += $"{tarot.name}, ";
            }

            print($"player {player.playerIndex + 1} has {player.cfg.Mask.name} and " + tarotString);
        }
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
        AudioManager.Instance?.uiAppear();
        showUI.UpdateMask(currentMask);
    }
}
