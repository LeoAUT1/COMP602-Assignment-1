using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Player : Singleton<Player>
{
    public string playerName = "Adventurer";

    private float powerCurve = 1.1f; //The power term for how much exp should be required for each future level.
    private int baseExp = 20; // Base exp for each level

    private int experience;
    private int playerLevel = 1;
    [SerializeField] PlayerCombat playerCombat;

    private Board board;


    private BoardTile currentTile;
    [SerializeField] private int tileIndex;

    public void ResetPlayer()
    {
        playerLevel = 1;
        experience = 0;
        currentTile = null;
        tileIndex = 0;
    }

    private bool LevelUp(int exp)
    {
        float plev = playerLevel;

        if (exp >= baseExp * Mathf.Pow(plev, powerCurve))
        {
            playerLevel++;
            return true;
        }

        return false;
    }

    public void AddExperience(int amount)
    {
        experience += amount;

        bool hasLeveledUp = LevelUp(experience);
        //display some messsage for leveling up, or something
        if (hasLeveledUp) {
            Debug.Log($"Player is now level: {playerLevel}");

            playerCombat.SetDexterity(playerCombat.GetDexterity() +1 );
            playerCombat.SetIntelligence(playerCombat.GetIntelligence() + 1);
            playerCombat.SetStrength(playerCombat.GetStrength() + 1);
        }

        if (board != null)
        {
            board.UpdatePlayerStatsUi();
        }
    }

    public int GetExperience()
    {
        return experience;
    }

    public int GetLevel()
    {
        return playerLevel;
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
        this.tileIndex = tile.GetIndex();
    }

    public PlayerCombat GetPlayerCombat()
    {
        return playerCombat;
    }

    public void SetBoard(Board b)
    {
        board = b;
    }
}
