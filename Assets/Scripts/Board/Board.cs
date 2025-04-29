using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Needed for FindPaths logic potentially
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
    [SerializeField] public PlayerAnimator playerAnimator; // Assign in Inspector
    [SerializeField] private GameObject playerPiecePrefab;
    [SerializeField] private GameObject selectionarrowPrefab;
    private GameObject playerPiece;

    private bool isNewGame = true;
    [SerializeField] private TextMeshProUGUI playerStats;

    private bool isAwaitingPathChoice = false;

    // --- Pathfinding Data ---
    private List<List<BoardTile>> currentPathChoices;
    private List<GameObject> pathArrows = new List<GameObject>();

    private void Start()
    {
        // Ensure tiles are fetched before potentially using them
        tiles = tileContainer.GetComponentsInChildren<BoardTile>();
        if (tiles == null || tiles.Length == 0)
        {
            Debug.LogError("Failed to find any BoardTile components in the tileContainer!");
            // Consider disabling the component or showing an error state
            return;
        }

        if (startTile != null && isNewGame)
        {
            Debug.Log("Is new game, setting player to first tile");
            // Ensure the startTile is actually part of the fetched tiles if needed,
            // or just trust the serialized reference.
            player.SetCurrentBoardTile(startTile);
        }
        else if (startTile == null && isNewGame)
        {
            Debug.LogError("New game started but Start Tile is not assigned in the Inspector!");
            // Handle error - maybe default to tiles[0]?
            if (tiles.Length > 0)
            {
                player.SetCurrentBoardTile(tiles[0]);
                Debug.LogWarning("Using first tile in container as start tile due to missing reference.");
            }
            else
            {
                Debug.LogError("Cannot set start tile - no tiles found and no start tile assigned.");
                return; // Critical error
            }
        }


        // Instantiate and place the player piece instantly at the start
        playerPiece = Instantiate(this.playerPiecePrefab);


        if (playerAnimator != null)
        {
            playerAnimator.SetPlayerPiece(playerPiece);
        }
        else
        {
            Debug.LogError("PlayerAnimator reference not set on Board!");
            return;
        }


        if (player.GetCurrentBoardTile() == null)
        {
            player.SetCurrentBoardTile(this.GetTileByIndex(player.GetTileIndex()));
        }

        BoardTile initialPlayerTile = player.GetCurrentBoardTile(); // Get the tile set above
        if (initialPlayerTile != null)
        {
            playerAnimator.MovePlayerPieceInstantly(initialPlayerTile);
        }
        else
        {
            Debug.LogError("Cannot determine initial player tile for placement even after attempting setup.");
        }

        UpdatePlayerStatsUi(player);
    }

    private void UpdatePlayerStatsUi(Player player)
    {
        if (playerStats != null && player != null)
        {
            playerStats.text = $"Experience: {player.GetExperience()}\n";
            playerStats.text += $"Coins: {player.GetCoins()}\n";
        }
        else
        {
            Debug.LogWarning("PlayerStats UI or Player reference is missing.");
        }
    }

    private int RollTheDice()
    {
        int diceRoll = UnityEngine.Random.Range(1, 7); // Standard d6 roll
        Debug.Log($"Dice Roll: {diceRoll}");
        return diceRoll;
    }

    // --- Main Player Action ---
    public void PlayerAction_RollAndMove()
    {
        // Prevent action if already moving or waiting for path selection
        if (playerAnimator != null && playerAnimator.IsAnimating)
        {
            Debug.LogWarning("Player is already moving.");
            return;
        }
        if (isAwaitingPathChoice)
        {
            Debug.LogWarning("Player is currently selecting a path.");
            return;
        }

        int roll = RollTheDice();
        BoardTile currentTile = player.GetCurrentBoardTile();

        if (currentTile == null)
        {
            Debug.LogError("Player's current tile is null. Cannot roll and move.");
            return;
        }

        // --- Use the new FindPaths function ---
        List<List<BoardTile>> availablePaths = FindPaths(currentTile, roll);

        // --- Handle the results ---
        if (availablePaths == null || availablePaths.Count == 0)
        {
            // No valid paths found for the rolled steps
            Debug.Log($"No valid paths found for a roll of {roll} from tile {currentTile.name}. Player stays put.");
            // Potentially end turn or trigger other logic for being stuck
        }
        else if (availablePaths.Count == 1)
        {
            // Exactly one path found - proceed automatically
            Debug.Log($"One path found for roll {roll}. Moving automatically.");
            List<BoardTile> pathToTake = availablePaths[0];

            if (pathToTake.Count > 1) // Ensure the path involves actual movement
            {
                playerAnimator.AnimateMove(pathToTake, () => HandleMovementComplete(pathToTake));
            }
            else
            {
                Debug.Log("Path found has only one tile (start tile). No movement needed.");
            }
        }
        else // availablePaths.Count > 1
        {
            // Multiple paths found - require player input
            Debug.Log($"Multiple ({availablePaths.Count}) paths found for roll {roll}. Awaiting player selection.");
            isAwaitingPathChoice = true;
            currentPathChoices = availablePaths; // Store the choices

            VisualizePathChoices(currentPathChoices);
        }
    }

    public void SelectPath(int index)
    {
        List<BoardTile> path = currentPathChoices[index];
        SelectPathAndMove(path);
    }

    // --- NEW: Method called by Input System when player selects a path ---
    private void SelectPathAndMove(List<BoardTile> chosenPath)
    {
        if (!isAwaitingPathChoice)
        {
            Debug.LogWarning("SelectPathAndMove called, but not currently awaiting path choice.");
            return;
        }
        if (chosenPath == null || chosenPath.Count == 0)
        {
            Debug.LogError("SelectPathAndMove called with an invalid path.");
            // Maybe re-enable input or handle error? For now, just reset state.
            ClearPathVisualizations(); // Clean up visuals
            isAwaitingPathChoice = false;
            currentPathChoices = null;
            return;
        }

        Debug.Log($"Player selected path ending at {chosenPath[chosenPath.Count - 1].name}.");

        isAwaitingPathChoice = false;
        currentPathChoices = null; // Clear stored choices

        ClearPathVisualizations();


        // Start movement along the selected path
        if (chosenPath.Count > 1)
        {
            playerAnimator.AnimateMove(chosenPath, () => HandleMovementComplete(chosenPath));
        }
        else
        {
            Debug.LogWarning("Selected path has only one tile. No movement animation needed.");
            // If landing on the same tile should trigger something, do it here.
            HandleMovementComplete(chosenPath);

        }
    }

    private void HandleMovementComplete(List<BoardTile> path)
    {
        if (path == null || path.Count == 0) return; // Safety check

        BoardTile finalTile = path[path.Count - 1];
        Debug.Log($"Movement finished. Final tile: {finalTile.name}");
        player.SetCurrentBoardTile(finalTile); // Update logical position

        // Check for encounters AFTER movement is complete
        EncounterData encounter = finalTile.GetEncounter();
        if (encounter != null /* && finalTile.IsEncounterUntriggered() */)
        {
            Debug.Log($"Landed on tile with encounter: {encounter.name}");
            // finalTile.MarkEncounterTriggered();
            StartEncounter(encounter);
        }
        else
        {
            Debug.Log($"Landed on tile {finalTile.name} with no encounter or encounter already triggered.");
            // Maybe signal end of turn here if no encounter
        }
    }

    private void VisualizePathChoices(List<List<BoardTile>> paths)
    {
        int index = 0;
        foreach (List<BoardTile> path in paths) {
            BoardTile finalPathTile = path.Last();
            GameObject go = Instantiate(selectionarrowPrefab, finalPathTile.transform.position, Quaternion.identity, finalPathTile.transform);
            pathArrows.Add(go); // Make a list of our arrows so we can clean them up later.
            ClickablePathingArrow arrow = go.GetComponent<ClickablePathingArrow>();
            arrow.pathIndex = index;
            arrow.SetBoard(this);
            index++;

        }
    }

    private void ClearPathVisualizations()
    {
        foreach (GameObject go in pathArrows)
        {
            if (go != null) {
                Destroy(go);
            }
        }

        pathArrows.Clear();
    }

    public List<List<BoardTile>> FindPaths(BoardTile startTile, int steps)
    {
        List<List<BoardTile>> allPathsFound = new List<List<BoardTile>>();
        if (startTile == null || steps < 0)
        {
            Debug.LogError($"Invalid input for FindPaths: startTile={startTile}, steps={steps}");
            return allPathsFound;
        }
        if (steps == 0)
        {
            allPathsFound.Add(new List<BoardTile> { startTile });
            return allPathsFound;
        }
        List<BoardTile> initialPath = new List<BoardTile> { startTile };
        FindPathsRecursive(startTile, initialPath, steps, allPathsFound);
        return allPathsFound;
    }

    private void FindPathsRecursive(BoardTile currentTile, List<BoardTile> currentPath, int stepsRemaining, List<List<BoardTile>> allPathsFound)
    {
        if (stepsRemaining == 0)
        {
            allPathsFound.Add(new List<BoardTile>(currentPath));
            return;
        }

        BoardTile previousTile = (currentPath.Count >= 2) ? currentPath[currentPath.Count - 2] : null;
        List<BoardTile> potentialNeighbors = new List<BoardTile>();

        // Add forward connections
        if (currentTile.nextTiles != null) potentialNeighbors.AddRange(currentTile.nextTiles);
        // Add backward connections (Requires BoardTile.PreviousTiles property)
        if (currentTile.PreviousTiles != null) potentialNeighbors.AddRange(currentTile.PreviousTiles);

        foreach (BoardTile neighbor in potentialNeighbors)
        {
            if (neighbor == null || neighbor == previousTile) continue; // Skip nulls and immediate U-turns

            List<BoardTile> newPath = new List<BoardTile>(currentPath);
            newPath.Add(neighbor);
            FindPathsRecursive(neighbor, newPath, stepsRemaining - 1, allPathsFound);
        }
    }

    private void StartEncounter(EncounterData encounter)
    {
        Debug.Log($"Starting encounter: {encounter.name}");
        // Assuming GameManager and SceneLoader are singletons or accessible
        if (GameManager.Instance != null) GameManager.Instance.SetCurrentEncounter(encounter);
        else Debug.LogError("GameManager.Instance is null!");

        if (SceneLoader.Instance != null) SceneLoader.Instance.LoadCombatScene();
        else Debug.LogError("SceneLoader.Instance is null!");
    }

    // GetTileByIndex, SetIsNewGame, SetPlayer, UpdateEncounterData remain largely the same
    public BoardTile GetTileByIndex(int index)
    {
        // Added check for tiles array initialization
        if (tiles == null)
        {
            Debug.LogError("Tiles array not initialized in GetTileByIndex.");
            return null;
        }
        if (index >= 0 && index < tiles.Length)
        {
            // Optional: Check if tiles[index] itself is null if that's possible
            return tiles[index];
        }
        Debug.LogWarning($"Invalid tile index requested: {index}. Max index is {tiles.Length - 1}");
        return null;
    }

    public void SetIsNewGame(bool b) { this.isNewGame = b; }
    public void SetPlayer(Player player) { this.player = player; }

    public void UpdateEncounterData(EncounterData encounterData)
    {
        int index = player.GetTileIndex(); // Assumes player has GetTileIndex()
        BoardTile tile = GetTileByIndex(index);
        if (tile != null)
        {
            tile.SetEncounterData(encounterData);
            Debug.Log($"Updated encounter data on tile {index}.");
        }
        else
        {
            Debug.LogError($"Could not find tile at index {index} to update encounter data.");
        }
    }


    public void MovePlayerSteps(int steps)
    {
        if (playerAnimator.IsAnimating || isAwaitingPathChoice)
        {
            Debug.LogWarning("Cannot MovePlayerSteps while moving or awaiting choice.");
            return;
        }
        if (steps <= 0)
        {
            Debug.LogWarning($"MovePlayerSteps called with non-positive value: {steps}.");
            return;
        }
        Debug.Log($"Attempting to force move player {steps} steps forward (using default path).");

        List<BoardTile> path = GetSimplePathAhead(steps); // Renamed for clarity

        if (path != null && path.Count > 1)
        {
            playerAnimator.AnimateMove(path, () => HandleMovementComplete(path));
        }
        else
        {
            Debug.LogWarning($"Could not find a simple forward path for {steps} steps.");
        }
    }

    private List<BoardTile> GetSimplePathAhead(int steps)
    {
        List<BoardTile> path = new List<BoardTile>();
        BoardTile currentTile = player.GetCurrentBoardTile();
        if (currentTile == null || steps <= 0) return path; // Return empty path

        path.Add(currentTile);
        BoardTile nextTileInPath = currentTile;
        for (int i = 0; i < steps; i++)
        {
            nextTileInPath = nextTileInPath.GetNextTile(); // Assumes GetNextTile(0) is the default
            if (nextTileInPath == null) return path; // Reached end or dead end
            path.Add(nextTileInPath);
        }
        return path;
    }

}
