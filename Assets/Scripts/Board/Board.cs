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

        PlayerSetup();

        UpdatePlayerStatsUi();
    }

    private void PlayerSetup()
    {
        Player.Instance.SetBoard(this);
        player = Player.Instance; //This is probably stupid and should be refactored
        player.SetCurrentBoardTile(this.GetTileByIndex(player.GetTileIndex()));

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

        //Move our player game board piece to their current tile.
        if (player.GetCurrentBoardTile() != null)
        {
            playerAnimator.MovePlayerPieceInstantly(player.GetCurrentBoardTile());
        }
        else
        {
            Debug.LogError("Cannot determine initial player tile for placement even after attempting setup.");
        }
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

    public void PlayerAction_RollAndMove(int roll)
    {
        Debug.Log($"Player rolled {roll}. Calculating path...");

        // Prevent action if already moving or waiting for path selection
        // Note: Buttons are already disabled by OnRollButtonClicked/RollTheDiceCoroutine
        if (playerAnimator != null && playerAnimator.IsAnimating)
        {
            Debug.LogWarning("Player is already moving. Move cancelled.");
            // If animation was somehow interrupted, maybe re-enable buttons? Risky.
            // Best to let the current animation complete and call HandleMovementComplete.
            return;
        }
        if (isAwaitingPathChoice)
        {
            Debug.LogWarning("Player is currently selecting a path. Move cancelled.");
            // Similar to above, let the path choice resolve.
            return;
        }

        BoardTile currentTile = player.GetCurrentBoardTile();
        if (currentTile == null)
        {
            Debug.LogError("Player's current tile is null. Cannot roll and move.");
            EnableBoardButtons(); // Re-enable buttons if state is broken.
            return;
        }

        if (roll <= 0)
        {
            Debug.Log($"Roll was {roll}. No movement required.");
            // Turn effectively ends, re-enable buttons.
            EnableBoardButtons();
            return;
        }

        // --- Path Generation ---
        List<BoardTile> path = new List<BoardTile>();
        path.Add(currentTile); // Path starts at the current tile

        BoardTile nextTileInPath = currentTile; // Use this to iterate forward

        for (int i = 0; i < roll; i++)
        {
            // Get the next tile based on the *last* one successfully added.
            // Assumes GetNextTile() provides the single, default forward path.
            BoardTile next = nextTileInPath.GetNextTile();

            if (next == null)
            {
                // Reached the end of the board before completing all steps.
                Debug.Log($"Reached end of the board after {i} steps (rolled {roll}). Path terminates here.");
                break; // Stop adding tiles
            }

            path.Add(next);
            nextTileInPath = next; // Move forward for the next iteration
        }
        // --- End Path Generation ---


        // --- Initiate Animation ---
        // Only animate if the path contains more than just the starting tile.
        if (path.Count > 1)
        {
            Debug.Log($"Generated path with {path.Count} tiles. Starting animation.");
            // Buttons remain disabled. HandleMovementComplete will re-enable them
            // after the animation finishes and encounters are checked.
            playerAnimator.AnimateMove(path, () => HandleMovementComplete(path));
        }
        else
        {
            // This case happens if roll > 0 but the player is already at the very last tile.
            Debug.Log("Path generation resulted in no movement (already at the end of the board?).");
            // No animation needed, re-enable buttons as the turn effectively ends here.
            EnableBoardButtons();
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

    // GetTileByIndex, SetPlayer, UpdateEncounterData remain largely the same
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
