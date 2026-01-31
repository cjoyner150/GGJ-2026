
using System.Collections.Generic;
using UnityEngine;

public class PlayerTurnManager : MonoBehaviour
{
    private List<PlayerGold> players;
    public int currentPlayerIndex { get; private set; }

    public PlayerGold currentPlayer
    {
        get
        {
            if (players.Count == 0)
            {
                return null;
            }
            return players[currentPlayerIndex];
        }
    }

    public PlayerTurnManager(List<PlayerGold> players)
    {
        this.players = players;
        currentPlayerIndex = 0;
    }
    public void nextPlayerTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
    }
    public void removePlayer(PlayerGold player)
    {
        players.Remove(player);
        if (currentPlayerIndex >= players.Count)
        {
            currentPlayerIndex = 0;
        }
    }
    public bool isOnlyOnePlayerLeft()
    {
        return players.Count == 1;
    }
}
