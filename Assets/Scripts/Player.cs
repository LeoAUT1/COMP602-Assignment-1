using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Player : Singleton<Player>
{
    private int experience;
    [SerializeField] protected int coins;
    [SerializeField] PlayerCombat playerCombat;


    private BoardTile currentTile;
    private int tileIndex;

    public void ResetPlayer()
    {
        experience = 0;
        coins = 0;
        currentTile = null;
        tileIndex = 0;
    }    

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

    public void AddExperience(int amount)
    {
        experience += amount;
    }

    public int GetExperience()
    {
        return experience;
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

    public PlayerCombat GetPlayerCombat()
    {
        return playerCombat;
    }
}
