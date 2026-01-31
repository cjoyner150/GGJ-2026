using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BiddingController : MonoBehaviour
{
    public Action<PlayerGold, Masks> OnMaskAssigned;
    public Action<PlayerGold, TarrotCard> OnTarrotAssigned;

    [Header("Masks Bidding Settings")]
    public int maskStartingBid = 10;
    public List<Masks> availableMasks;
    [Header("Tarrot Cards Bidding Settings")]
    public int tarrotStartingBid = 15;
    public List<TarrotCard> availableTarrotCards;

    private List<PlayerGold> players;
    private MaskDeck maskDeck;
    private int maskdeckIndex;

    private BiddingSession maskBidding;
    private NormalBidding tarrotBidding;

    public bool isMaskbidding { get; private set; }
    public bool isTarrotBidding { get; private set; }
    public void Begin(List<PlayerGold> activeplayers)
    {
        this.players = activeplayers;
        maskDeck = new MaskDeck(availableMasks);
        maskdeckIndex = 0;
        StartMaskBidding();
    }
    private void StartMaskBidding()
    {
        if (maskdeckIndex >= availableMasks.Count)
        {
            startTarrotBidding();
            return;
        }
        Masks currentmask = maskDeck.drawMask();
        maskBidding = new BiddingSession(players);
        maskBidding.OnMaskWinner += winner =>
            {
                OnMaskAssigned?.Invoke(winner, currentmask);
                maskdeckIndex++;
                StartMaskBidding();
            };
    }
    private void startTarrotBidding()
    {
        isMaskbidding = false;
        isTarrotBidding = true;
        tarrotBidding = new NormalBidding(bidIncrement);
    }
    private void PlayerBid(PlayerGold player){
        if (isMaskbidding)
        {
            maskBidding.TryBid(player, maskStartingBid);
        }
        else if (isTarrotBidding)
        {
            tarrotBidding.Trybid(player);
        }
    }
    private void endTarrotBidding()
    {
        isTarrotBidding = false;
        tarrotBidding.EndBidding();
    }
    private void AssigntarrotToWinner(PlayerGold winner, TarrotCard card)
    {
        OnTarrotAssigned?.Invoke(winner, card);
    }
}
