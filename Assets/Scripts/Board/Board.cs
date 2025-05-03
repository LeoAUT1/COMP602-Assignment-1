using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Needed for FindPaths logic potentially
using TMPro;
using Unity.VisualScripting;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.UI;

// Enum GameBoardState remains the same

public class Board : MonoBehaviour
{
    // ... (Keep all existing fields and methods like Start, UpdatePlayerStatsUi, RollTheDice, etc.) ...

    [SerializeField] private BoardTile startTile;
    [SerializeField] private GameObject tileContainer;
    private BoardTile[] tiles;

    [SerializeField] private Player player;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] public PlayerAnimator playerAnimator; // Assign in Inspector
    [SerializeField] private GameObject playerPiecePrefab;
    [SerializeField] private GameObject selectionarrowPrefab;
    private GameObject playerPiece;

    private bool isNewGame = true;
    [SerializeField] private TextMeshProUGUI playerStats;

    private bool isAwaitingPathChoice = false;

    [SerializeField] private GameObject die;

    [SerializeField] private GameObject boardInteractionButtons;

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

        if (player != null)
        {
            player.SetBoard(this);
        }

        UpdatePlayerStatsUi();
    }

    public void UpdatePlayerStatsUi()
    {
        if (playerStats != null && player != null)
        {
            playerStats.text = $"LVL {player.GetLevel()} | {player.GetExperience()} xp\n";
            playerStats.text += $"Coins: {player.GetCoins()}\n";
        }
        else
        {
            Debug.LogWarning("PlayerStats UI or Player reference is missing.");
        }
    }

    public void RollTheDice(System.Action<int> callback)
    {
        Vector3 dieSpawnPoint = Vector3.Lerp(Camera.main.transform.position, playerPiece.transform.position, 0.5f);

        // Vector3 dieSpawnPoint = cameraPosition + Camera.main.transform.forward * distanceFromCamera;

        GameObject diceObj = Instantiate(die, dieSpawnPoint, Quaternion.identity);
        Dice dice = diceObj.GetComponentInChildren<Dice>();

        // Subscribe to the dice roll event
        dice.OnDiceRollComplete += (result) => {
            callback(result);
            Destroy(diceObj, 2f); // Destroy our dice after 2 seconds
        };

        dice.PhysicalRoll();
    }

    // Using a coroutine
    public IEnumerator RollTheDiceCoroutine(System.Action<int> onComplete)
    {
        int result = 0;
        bool rollComplete = false;

        RollTheDice((rollResult) => {
            result = rollResult;
            rollComplete = true;
            PlayerAction_RollAndMove(result);
        });

        // Wait until the roll is complete
        yield return new WaitUntil(() => rollComplete);

        onComplete(result);
    }

    // Usage example:
    public void OnRollButtonClicked()
    {
        StartCoroutine(RollTheDiceCoroutine((result) => {
            Debug.Log($"Player rolled: {result}");
            // Do something with the result
        }));
        DisableBoardButtons();
    }


    // --- Main Player Action ---
    public void PlayerAction_RollAndMove(int roll)
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
            EnableBoardButtons(); // Re-enable buttons if stuck
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
                EnableBoardButtons(); // Re-enable buttons if no movement
            }
        }
        else // availablePaths.Count > 1
        {
            // Multiple paths found - require player input
            Debug.Log($"Multiple ({availablePaths.Count}) paths found for roll {roll}. Awaiting player selection.");
            isAwaitingPathChoice = true;
            currentPathChoices = availablePaths; // Store the choices

            VisualizePathChoices(currentPathChoices);
            // Buttons remain disabled until a path is chosen
        }
    }

    public void SelectPath(int index)
    {
        if (!isAwaitingPathChoice || currentPathChoices == null || index < 0 || index >= currentPathChoices.Count)
        {
            Debug.LogWarning($"Invalid attempt to select path with index {index}. State: awaiting={isAwaitingPathChoice}, choices count={currentPathChoices?.Count}");
            // Optionally re-enable buttons or reset state if this happens unexpectedly
            // EnableBoardButtons();
            return;
        }
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
            EnableBoardButtons(); // Re-enable buttons on error
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
            HandleMovementComplete(chosenPath); // Still call HandleMovementComplete for potential encounters etc.
            // Buttons will be re-enabled in HandleMovementComplete if no encounter starts
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
        if (encounter != null /* && finalTile.IsEncounterUntriggered() */) // Add encounter trigger logic if needed
        {
            Debug.Log($"Landed on tile with encounter: {encounter.name}");
            // finalTile.MarkEncounterTriggered(); // Add if you only want encounters once
            StartEncounter(encounter);
            // Buttons remain disabled as an encounter is starting
            return; // Don't re-enable buttons yet
        }
        else
        {
            Debug.Log($"Landed on tile {finalTile.name} with no encounter or encounter already triggered.");
            // Player has finished moving and there's no encounter, re-enable buttons for next action.
            EnableBoardButtons();
        }
    }

    private void VisualizePathChoices(List<List<BoardTile>> paths)
    {
        ClearPathVisualizations(); // Clear any previous arrows first
        int index = 0;
        foreach (List<BoardTile> path in paths)
        {
            if (path == null || path.Count == 0) continue; // Skip invalid paths

            BoardTile finalPathTile = path.Last();
            if (finalPathTile == null) continue; // Skip if final tile is somehow null

            GameObject go = Instantiate(selectionarrowPrefab, finalPathTile.transform.position, Quaternion.identity, finalPathTile.transform);
            pathArrows.Add(go); // Make a list of our arrows so we can clean them up later.
            ClickablePathingArrow arrow = go.GetComponent<ClickablePathingArrow>();
            if (arrow != null)
            {
                arrow.pathIndex = index;
                arrow.SetBoard(this);
            }
            else
            {
                Debug.LogError($"Prefab {selectionarrowPrefab.name} is missing ClickablePathingArrow script!", selectionarrowPrefab);
                Destroy(go); // Destroy the useless arrow
            }
            index++;
        }
    }

    private void ClearPathVisualizations()
    {
        foreach (GameObject go in pathArrows)
        {
            if (go != null)
            {
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
        // If steps is 0, the only path is the start tile itself.
        if (steps == 0)
        {
            allPathsFound.Add(new List<BoardTile> { startTile });
            return allPathsFound;
        }

        List<BoardTile> initialPath = new List<BoardTile> { startTile };
        FindPathsRecursive(startTile, initialPath, steps, allPathsFound);

        // Post-processing: Remove duplicate paths if the logic somehow creates them
        // This can happen if multiple recursive calls end on the same tile via the new rule.
        if (allPathsFound.Count > 1)
        {
            allPathsFound = allPathsFound
                .GroupBy(path => path.LastOrDefault()) // Group by the final tile
                .Select(group => group.First())        // Select the first path found for each unique end tile
                .ToList();
        }


        return allPathsFound;
    }

    // --- MODIFIED RECURSIVE FUNCTION ---
    private void FindPathsRecursive(BoardTile currentTile, List<BoardTile> currentPath, int stepsRemaining, List<List<BoardTile>> allPathsFound)
    {
        // Base Case 1: Exact number of steps taken
        if (stepsRemaining == 0)
        {
            // Check if this exact path (ending tile) has already been added. Avoid duplicates.
            // This check might be redundant depending on the post-processing in FindPaths,
            // but can optimize recursion slightly.
            if (!allPathsFound.Any(p => p.SequenceEqual(currentPath)))
            {
                allPathsFound.Add(new List<BoardTile>(currentPath));
            }
            return;
        }

        BoardTile previousTile = (currentPath.Count >= 2) ? currentPath[currentPath.Count - 2] : null;
        List<BoardTile> potentialNeighbors = new List<BoardTile>();
        bool canMoveForward = false;

        // Check forward moves first to determine if we *can* move forward
        if (currentTile.nextTiles != null)
        {
            foreach (BoardTile next in currentTile.nextTiles)
            {
                if (next != null) // Check if *any* forward tile exists
                {
                    potentialNeighbors.Add(next); // Add for exploration later
                    if (next != previousTile) // Check if it's a *valid* forward move (not immediate U-turn)
                    {
                        canMoveForward = true;
                        // Don't break here, add all potential forward neighbors
                    }
                }
            }
        }

        // Add backward moves (if applicable and allowed by game rules)
        if (currentTile.PreviousTiles != null)
        {
            foreach (BoardTile prev in currentTile.PreviousTiles)
            {
                if (prev != null)
                {
                    // Avoid adding duplicates if a tile is in both lists
                    if (!potentialNeighbors.Contains(prev))
                    {
                        potentialNeighbors.Add(prev);
                    }
                }
            }
        }

        // --- NEW Base Case 2 ---
        // If we cannot move forward from the current tile (reached end of a branch/board)
        // AND we still have steps remaining, this path is considered valid and ends here.
        if (!canMoveForward && stepsRemaining > 0)
        {
            // Check if this exact path (ending tile) has already been added.
            if (!allPathsFound.Any(p => p.SequenceEqual(currentPath)))
            {
                allPathsFound.Add(new List<BoardTile>(currentPath));
            }
            return; // Stop exploring this branch, path is complete.
        }

        // Recursive Step: Explore valid neighbors
        bool exploredAnyNeighbor = false;
        foreach (BoardTile neighbor in potentialNeighbors)
        {
            // Skip nulls and immediate U-turns
            if (neighbor == null || neighbor == previousTile) continue;

            // Prevent cycles within a single path search if needed (optional, depends on board complexity)
            // if (currentPath.Contains(neighbor)) continue;

            List<BoardTile> newPath = new List<BoardTile>(currentPath);
            newPath.Add(neighbor);
            FindPathsRecursive(neighbor, newPath, stepsRemaining - 1, allPathsFound);
            exploredAnyNeighbor = true;
        }

        // --- Implicit Base Case 3 ---
        // If we have steps remaining, but there were NO valid neighbors to explore
        // (e.g., only neighbor was previousTile, or all neighbors were null),
        // this path simply terminates without being added to allPathsFound, which is correct.
        // The `!exploredAnyNeighbor && stepsRemaining > 0` condition is implicitly handled.
    }
    // --- END OF MODIFIED FUNCTION ---


    private void StartEncounter(EncounterData encounter)
    {
        Debug.Log($"Starting encounter: {encounter.name}, TYPE: {encounter.encounterType}");

        if (encounter.encounterType == EncounterType.COMBAT)
        {
            // Assuming GameManager and SceneLoader are singletons or accessible
            if (GameManager.Instance != null) GameManager.Instance.SetCurrentEncounter(encounter);
            else Debug.LogError("GameManager.Instance is null!");

            if (SceneLoader.Instance != null) SceneLoader.Instance.LoadCombatScene();
            else Debug.LogError("SceneLoader.Instance is null!");
        }
        else
        {
            if (encounter.prefab == null)
            {
                Debug.LogError($"No prefab for the non-combat encounter: {encounter.name}");
                EnableBoardButtons(); // Re-enable buttons if encounter fails to start
                return;
            }
            else
            {
                // Instantiate and setup the non-combat encounter
                GameObject encounterInstance = Instantiate(encounter.prefab);
                GenericNonCombatEncounter encounterScript = encounterInstance.GetComponent<GenericNonCombatEncounter>();

                if (encounterScript != null)
                {
                    encounterScript.SetBoard(this);
                    // Add any other necessary setup for the encounter here
                }
                else
                {
                    Debug.LogError($"Encounter prefab {encounter.prefab.name} is missing GenericNonCombatEncounter script!", encounter.prefab);
                    Destroy(encounterInstance); // Clean up useless instance
                    EnableBoardButtons(); // Re-enable buttons if encounter script is missing
                }
            }
        }
        // Buttons remain disabled while encounter is active. The encounter system
        // should call EnableBoardButtons() when it finishes.
    }

    // GetTileByIndex, SetIsNewGame, SetPlayer, UpdateEncounterData remain largely the same
    public BoardTile GetTileByIndex(int index)
    {
        // Added check for tiles array initialization
        if (tiles == null)
        {
            Debug.LogError("Tiles array not initialized in GetTileByIndex.");
            // Attempt to initialize if null and in editor? Or just fail.
            tiles = tileContainer?.GetComponentsInChildren<BoardTile>();
            if (tiles == null || tiles.Length == 0)
            {
                Debug.LogError("Failed to re-initialize tiles array.");
                return null;
            }
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
        if (player == null)
        {
            Debug.LogError("Cannot update encounter data: Player reference is missing.");
            return;
        }
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
        // This function seems designed for forced movement along a default path.
        // It might need adjustment depending on how you want forced movement
        // to interact with the end-of-board rule. Currently, it uses GetSimplePathAhead.

        DisableBoardButtons(); // Disable buttons during forced move
        if (playerAnimator.IsAnimating || isAwaitingPathChoice)
        {
            Debug.LogWarning("Cannot MovePlayerSteps while moving or awaiting choice.");
            EnableBoardButtons(); // Re-enable if action is blocked
            return;
        }
        if (steps <= 0)
        {
            Debug.LogWarning($"MovePlayerSteps called with non-positive value: {steps}.");
            EnableBoardButtons(); // Re-enable if steps invalid
            return;
        }
        Debug.Log($"Attempting to force move player {steps} steps forward (using default path).");

        List<BoardTile> path = GetSimplePathAhead(steps); // Renamed for clarity

        if (path != null && path.Count > 1)
        {
            playerAnimator.AnimateMove(path, () => HandleMovementComplete(path));
            // HandleMovementComplete will re-enable buttons if no encounter occurs
        }
        else
        {
            Debug.LogWarning($"Could not find a simple forward path for {steps} steps. Player stays put.");
            // If the path is invalid or just the start tile, re-enable buttons.
            // HandleMovementComplete won't be called in this case.
            EnableBoardButtons();
        }
    }

    // This helper function needs to respect the board end as well.
    private List<BoardTile> GetSimplePathAhead(int steps)
    {
        List<BoardTile> path = new List<BoardTile>();
        BoardTile currentTile = player.GetCurrentBoardTile();
        if (currentTile == null || steps <= 0) return path; // Return empty path if invalid start or steps

        path.Add(currentTile);
        BoardTile nextTileInPath = currentTile;
        for (int i = 0; i < steps; i++)
        {
            // Assuming GetNextTile() gets the primary 'forward' tile.
            // You might need a more robust way if multiple 'nextTiles' exist.
            BoardTile next = nextTileInPath.GetNextTile(); // Assumes GetNextTile() returns the default next tile or null

            if (next == null)
            {
                // Reached the end of the line before completing all steps.
                // The path up to 'nextTileInPath' is the valid path.
                Debug.Log($"Reached end of simple path after {i} steps. Requested {steps}.");
                break; // Exit the loop, the path is complete up to the last valid tile.
            }
            path.Add(next);
            nextTileInPath = next; // Move to the next tile for the next iteration
        }
        return path; // Return the constructed path (might be shorter than 'steps')
    }

    public void EnableBoardButtons()
    {
        if (boardInteractionButtons != null)
        {
            boardInteractionButtons.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Attempted to enable board buttons, but reference is null.");
        }
    }

    public void DisableBoardButtons()
    {
        if (boardInteractionButtons != null)
        {
            boardInteractionButtons.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Attempted to disable board buttons, but reference is null.");
        }
    }

} // End of Board class
