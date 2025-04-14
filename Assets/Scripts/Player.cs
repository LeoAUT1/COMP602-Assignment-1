using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Singleton<Player>
{
    [SerializeField] protected int coins;
    [SerializeField] protected int indexOnGameBoard;
    private BoardTile currentTile;
    private PlayerCombat playerCombat = new();

    public void AddCoins(int amount)
    {
        coins += amount;
    }

    public void SubtractCoins(int amount)
    {
        coins -= amount;
    }

    public int GetCoins() {
        return coins;
    }

    public BoardTile GetCurrentBoardTile()
    {
        return currentTile;
    }

    public void SetCurrentBoardTile(BoardTile tile)
    {
        currentTile = tile;
    }
}
