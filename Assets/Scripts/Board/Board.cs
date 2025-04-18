using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

enum GameBoardState
{
    WaitingForPlayer,
    ResolvingEncounter
}

public class Board : MonoBehaviour
{
    [SerializeField] private BoardTile startTile;// The first game tile

    [SerializeField] private GameObject tileContainer;// Game object of which all the board tiles are children
    private BoardTile[] tiles; //The tiles themselves


    [SerializeField] private Player player;// The first game tile
    [SerializeField] private GameObject playerPiecePrefab; // The prefab in the Inspector
    private GameObject playerPiece; // Reference to the instantiated object //Player's representation on the game board

    private bool isNewGame = true;

    [SerializeField] private TextMeshProUGUI playerStats;

    private void Start()
    {
        tiles = tileContainer.GetComponentsInChildren<BoardTile>();

        if (startTile != null && isNewGame)
        {
            Debug.Log("Is new game, setting player to first tile");
            player.SetCurrentBoardTile(startTile);
        }

        playerPiece = Instantiate(this.playerPiecePrefab);
        int playerIndex = this.player.GetTileIndex();
        BoardTile playerTile = GetTileByIndex(playerIndex);

        MovePlayerToTile(playerTile, isNewGame);

        UpdatePlayerStatsUi(player);
    }

    private void UpdatePlayerStatsUi(Player player)
    {
        playerStats.text = $"Experience: {player.GetExperience()}\n";
        playerStats.text += $"Coins: {player.GetCoins()}\n";
    }

    private int RollTheDice()
    {
        int diceRoll = UnityEngine.Random.Range(1, 7);
        Debug.Log(diceRoll);
        return diceRoll;
    }

    public void ProcessPlayerTurn()
    {
        int roll = RollTheDice();

        AdvancePlayer(roll);
    }

    public void AdvancePlayer(int amount)
    {
        BoardTile boardTile = GetTileAhead(amount);

        Debug.Log(boardTile);

        MovePlayerToTile(boardTile, true);
    }

    public void MovePlayerPieceToTile(BoardTile boardTile)
    {
        // Debug logging to help identify the null reference
        Debug.Log($"playerPiece is {(playerPiece == null ? "NULL" : "not null")}");
        Debug.Log($"boardTile is {(boardTile == null ? "NULL" : "not null")}");

        this.playerPiece.transform.position = boardTile.transform.position;
    }

    public void MovePlayerToTile(BoardTile boardTile, bool triggerEncounter)
    {
        if ( boardTile != null)
        {
            MovePlayerPieceToTile(boardTile);
            BoardTile currentTile = boardTile;
            EncounterData encounter = currentTile.GetEncounter();
            player.SetCurrentBoardTile(currentTile);

            if (encounter != null && triggerEncounter)
            {
                StartEncounter(encounter);
            }
        }
    }

    private void StartEncounter(EncounterData encounter)
    {
        Debug.Log($"Starting encounter: {encounter}");

        GameManager.Instance.SetCurrentEncounter( encounter );

        SceneLoader.Instance.LoadCombatScene();
    }

    //Traverse the tiles
    public BoardTile GetTileAhead(int steps)
    {

        BoardTile currentTile = player.GetCurrentBoardTile();

        if (steps < 0)
        {
            Debug.LogError("Cannot get tile with negative steps");
            return null;
        }

        if (steps == 0)
        {
            // Return the current tile
            return currentTile;
        }

        // Traverse the linked list 'steps' times
        for (int i = 0; i < steps; i++)
        {
            // If there are no next tiles, we've reached the end
            if (currentTile.nextTiles.Count == 0)
            {
                Debug.LogWarning($"Reached end of board after {i} steps (requested {steps})");
                return currentTile;
            }

            // Move to the next tile (using the first path by default)
            currentTile = currentTile.GetNextTile();

            // If we hit a null tile, something's wrong with the board
            if (currentTile == null)
            {
                Debug.LogError($"Null tile encountered after {i} steps");
                return null;
            }
        }

        return currentTile;
    }

    public BoardTile GetTileByIndex(int index)
    {
        return tiles[index];
    }

    public void SetIsNewGame(bool b)
    {
        this.isNewGame = b;
    }

    public void SetPlayer(Player player)
    {
        this.player = player;
    }

    public void UpdateEncounterData(EncounterData encounterData)
    {
        int index = player.GetTileIndex();
        BoardTile tile = tiles[index];
        tile.SetEncounterData(encounterData);
    }
}
