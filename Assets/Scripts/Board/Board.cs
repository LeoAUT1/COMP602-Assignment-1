using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Enum GameBoardState remains the same

public class Board : MonoBehaviour
{
    [SerializeField] private BoardTile startTile;
    [SerializeField] private GameObject tileContainer;
    private BoardTile[] tiles;

    [SerializeField] private Player player;
    [SerializeField] private GameObject playerPiecePrefab;
    private GameObject playerPiece;

    private bool isNewGame = true;
    [SerializeField] private TextMeshProUGUI playerStats;

    [SerializeField] private float moveDurationPerTile = 0.5f; // Time in seconds to move between two tiles

    private bool isMoving = false; // Flag to prevent concurrent moves

    private void Start()
    {
        tiles = tileContainer.GetComponentsInChildren<BoardTile>();

        if (startTile != null && isNewGame)
        {
            Debug.Log("Is new game, setting player to first tile");
            player.SetCurrentBoardTile(startTile);
        }

        // Instantiate and place the player piece instantly at the start
        playerPiece = Instantiate(this.playerPiecePrefab);
        BoardTile initialPlayerTile = GetTileByIndex(player.GetTileIndex()); // Ensure we have the tile
        if (initialPlayerTile != null)
        {
            // Use the instant move for initial placement
            MovePlayerPieceInstantly(initialPlayerTile);
        }
        else
        {
            Debug.LogError("Cannot determine initial player tile for placement.");
        }


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
        Debug.Log($"Dice Roll: {diceRoll}");
        return diceRoll;
    }

    public void PlayerAction_RollAndMove()
    {
        if (isMoving)
        {
            Debug.LogWarning("Player is already moving.");
            return; // Don't allow actions while moving
        }

        int roll = RollTheDice();
        List<BoardTile> path = GetPathAhead(roll);

        if (path != null && path.Count > 1) // Only animate if there's more than one tile (i.e., actual movement)
        {
            StartCoroutine(AnimatePlayerMovement(path));
        }
        else if (path != null && path.Count == 1)
        {
            // If the path is just the current tile (roll 0 or can't move),
            // still potentially trigger encounter logic if needed, though GetPathAhead handles steps=0.
            // In this setup, no movement means no new encounter trigger.
            Debug.Log("No movement required.");
            // Optionally, trigger the current tile's encounter again if desired,
            // but the current logic implies encounters trigger on *landing*.
        }
        else
        {
            Debug.LogWarning("Failed to get path or path is empty.");
        }
    }

    // Instantly moves the player piece visuals. Used for setup.
    public void MovePlayerPieceInstantly(BoardTile boardTile)
    {
        if (playerPiece == null || boardTile == null || boardTile.playerPlacement == null)
        {
            Debug.LogError($"Cannot instantly move player piece. playerPiece: {playerPiece}, boardTile: {boardTile}, playerPlacement: {boardTile?.playerPlacement}");
            return;
        }
        playerPiece.transform.position = boardTile.playerPlacement.transform.position;

        // This looks superfluous but is necessary for click to move for debugging
        player.SetCurrentBoardTile(boardTile);
    }

    // Coroutine to animate movement along a path
    private IEnumerator AnimatePlayerMovement(List<BoardTile> path)
    {
        isMoving = true;
        Debug.Log($"Starting animation across {path.Count} tiles.");

        // Start from the second tile in the path (index 1)
        // The first tile (index 0) is the starting position.
        for (int i = 1; i < path.Count; i++)
        {
            BoardTile targetTile = path[i];
            Vector3 startPos = playerPiece.transform.position;
            Vector3 endPos = targetTile.playerPlacement.transform.position;
            float elapsedTime = 0f;

            Debug.Log($"Moving to tile {i}: {targetTile.name}");

            while (elapsedTime < moveDurationPerTile)
            {
                playerPiece.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / moveDurationPerTile);
                elapsedTime += Time.deltaTime;
                yield return null; // Wait for the next frame
            }

            // Ensure the piece is exactly at the target position
            playerPiece.transform.position = endPos;
        }

        // --- Animation Complete ---

        // Now, update the player's logical state to the final tile
        BoardTile finalTile = path[path.Count - 1];
        Debug.Log($"Movement finished. Final tile: {finalTile.name}");
        player.SetCurrentBoardTile(finalTile);

        // Check for and trigger encounter ONLY on the final tile
        EncounterData encounter = finalTile.GetEncounter();
        if (encounter != null)
        {
            Debug.Log($"Landed on tile with encounter: {encounter.name}");
            StartEncounter(encounter);
        }
        else
        {
            Debug.Log("Landed on tile with no encounter.");
        }

        isMoving = false; // Allow next move
    }


    private void StartEncounter(EncounterData encounter)
    {
        Debug.Log($"Starting encounter: {encounter.name}"); // Use encounter.name if it exists
        GameManager.Instance.SetCurrentEncounter(encounter);
        SceneLoader.Instance.LoadCombatScene();
    }

    // Renamed from GetTileAhead and modified to return the full path
    public List<BoardTile> GetPathAhead(int steps)
    {
        List<BoardTile> path = new List<BoardTile>();
        BoardTile currentTile = player.GetCurrentBoardTile();

        if (currentTile == null)
        {
            Debug.LogError("Player's current tile is null. Cannot determine path.");
            return null; // Or return empty list: new List<BoardTile>();
        }

        path.Add(currentTile); // Add the starting tile

        if (steps < 0)
        {
            Debug.LogError("Cannot get path with negative steps");
            return null; // Or return list with only start tile: path;
        }

        if (steps == 0)
        {
            // Return the path containing only the current tile
            return path;
        }

        BoardTile nextTileInPath = currentTile;
        // Traverse the linked list 'steps' times
        for (int i = 0; i < steps; i++)
        {
            // If there are no next tiles, we've reached the end
            if (nextTileInPath.nextTiles == null || nextTileInPath.nextTiles.Count == 0)
            {
                Debug.LogWarning($"Reached end of board after {i} steps (requested {steps}). Path ends at {nextTileInPath.name}");
                return path; // Return the path up to this point
            }

            // Move to the next tile (using the first path by default)
            nextTileInPath = nextTileInPath.GetNextTile(); // Assuming GetNextTile() gets the default next tile

            // If we hit a null tile, something's wrong with the board definition
            if (nextTileInPath == null)
            {
                Debug.LogError($"Null tile encountered after {i} steps while traversing from {path[path.Count - 1].name}");
                return path; // Return the path up to the point before the null tile
            }

            path.Add(nextTileInPath); // Add the valid next tile to the path
        }

        return path; // Return the complete path
    }

    public void MovePlayerSteps(int steps)
    {
        // Prevent starting a new move if one is already in progress.
        if (isMoving)
        {
            Debug.LogWarning("Player is already moving. Move command ignored.");
            return;
        }

        // Validate the input. Moving 0 or negative steps doesn't make sense here.
        if (steps <= 0)
        {
            Debug.LogWarning($"MovePlayerSteps called with non-positive value: {steps}. No movement will occur.");
            return;
        }

        Debug.Log($"Attempting to move player {steps} steps forward.");

        // Calculate the path the player will take.
        List<BoardTile> path = GetPathAhead(steps);

        // Check if a valid path with actual movement was found.
        // path.Count > 1 means there's at least a start and one destination tile.
        if (path != null && path.Count > 1)
        {
            // Start the animation coroutine using the calculated path.
            StartCoroutine(AnimatePlayerMovement(path));
        }
        else if (path != null && path.Count <= 1)
        {
            // This might happen if GetPathAhead determines no movement is possible (e.g., already at the end).
            Debug.LogWarning($"Calculated path for {steps} steps resulted in no actual movement (Path length: {path.Count}). Player remains on tile: {player.GetCurrentBoardTile()?.name}");
        }
        else
        {
            // GetPathAhead returned null, indicating an error during path calculation.
            Debug.LogError($"Failed to calculate path for {steps} steps. Check board configuration and player state.");
        }
    }

    public BoardTile GetTileByIndex(int index)
    {
        if (index >= 0 && index < tiles.Length)
        {
            return tiles[index];
        }
        Debug.LogError($"Invalid tile index requested: {index}");
        return null;
    }

    // SetIsNewGame, SetPlayer, UpdateEncounterData remain the same
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
        BoardTile tile = GetTileByIndex(index);
        if (tile != null)
        {
            tile.SetEncounterData(encounterData);
        }
        else
        {
            Debug.LogError($"Could not find tile at index {index} to update encounter data.");
        }
    }
}
