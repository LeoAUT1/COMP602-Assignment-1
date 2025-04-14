using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Start()
    {
        tiles = tileContainer.GetComponentsInChildren<BoardTile>();
        Debug.Log($"Found {tiles.Length} tiles");

        if (startTile != null)
        {
            player.SetCurrentBoardTile(startTile);
        } else
        {
            Debug.Log("No Start Tile");
        }
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

        MovePlayerToTile(boardTile);
    }

    public void MovePlayerToTile(BoardTile boardTile)
    {
        if ( boardTile != null)
        {
            player.transform.position = boardTile.transform.position;
            BoardTile currentTile = boardTile;
            Encounter encounter = currentTile.GetEncounter();
            player.SetCurrentBoardTile(currentTile);

            if (encounter != null)
            {
                StartEncounter(encounter);
            }
        }
    }

    private void StartEncounter(Encounter encounter)
    {
        Debug.Log($"Starting encounter: {encounter}");
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
}
