using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Player : Singleton<Player>
{
    [SerializeField] protected int coins;
    [SerializeField] protected int indexOnGameBoard;
    private BoardTile currentTile;
    private int tileIndex;
    //private PlayerCombat playerCombat = AddedComponent();

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

    public int GetTileIndex()
    {
        return tileIndex;
    }

    public void SetCurrentBoardTile(BoardTile tile)
    {
        currentTile = tile;
        this.tileIndex = tile.getIndex();
    }
}
