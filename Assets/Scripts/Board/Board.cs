using Codice.CM.Client.Differences;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


// Enum GameBoardState remains the same

public class Board : MonoBehaviour
{
    [SerializeField] private BoardTile startTile;
    [SerializeField] private GameObject tileContainer;
    private BoardTile[] tiles;

    [SerializeField] public PlayerAnimator playerAnimator; // Assign in Inspector
    [SerializeField] private GameObject playerPiecePrefab;
    private GameObject playerPiece;

    [SerializeField] private TextMeshProUGUI playerStats;

    [SerializeField] private GameObject boardInteractionButtons;

    [SerializeField] private float diceSpawnDistanceFromCam = 0.5f;

    [SerializeField] private PlayerInfoUI playerInfoUI;

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

        //This is not great but we are short on time

        PowerUpUIManager manager = this.GetComponentInChildren<PowerUpUIManager>();
        manager.RedrawPowerups();
    }

    private void PlayerSetup()
    {
        Player.Instance.SetBoard(this);
        Player.Instance.SetCurrentBoardTile(this.GetTileByIndex(Player.Instance.GetTileIndex()));

        // Instantiate and place the player piece instantly at the start
        playerPiece = Instantiate(this.playerPiecePrefab);

        Player.Instance.SetPlayerPiece(playerPiece);

        //Shrink down the player a bit
        playerPiece.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

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
        if (Player.Instance.GetCurrentBoardTile() != null)
        {
            playerAnimator.MovePlayerPieceInstantly(Player.Instance.GetCurrentBoardTile());
        }
        else
        {
            Debug.LogError("Cannot determine initial player tile for placement even after attempting setup.");
        }

        playerInfoUI.UpdateAll();
    }

    public void UpdatePlayerStatsUi()
    {
        if (playerStats != null)
        {
            playerStats.text = $"LVL {Player.Instance.GetLevel()} | {Player.Instance.GetExperience()} xp\n";
        }
        else
        {
            Debug.LogWarning("PlayerStats UI or Player reference is missing.");
        }

        playerInfoUI.UpdateAll();
    }

    public void OnRollButtonClicked()
    {
        if (Camera.main == null)
        {
            Debug.Log("Can't find our camera");
        }

        if (playerPiece == null)
        {
            Debug.Log("Cant find our player");
        }


        // We want to spawn our die halfway between the player and the camera, or thereabouts.
        Vector3 spawnPos = Vector3.Lerp(Camera.main.transform.position, playerPiece.transform.position, diceSpawnDistanceFromCam);

        StartCoroutine(DiceManager.Instance.RollTheDiceCoroutine(
            (diceRollResult) => {
                Debug.Log($"Player rolled: {diceRollResult}");
                PlayerAction_RollAndMove(diceRollResult); // Board calls its own method
            },
            spawnPos // Pass the calculated spawn position
        ));
        DisableBoardButtons();
    }


    public void PlayerAction_RollAndMove(int roll)
    {
        Debug.Log($"Player rolled {roll}. Creating path...");

        // Prevent action if already moving or waiting for path selection
        if (playerAnimator != null && playerAnimator.IsAnimating)
        {
            Debug.LogWarning("Player is already moving. Move cancelled.");
            return;
        }

        BoardTile currentTile = Player.Instance.GetCurrentBoardTile();
        if (currentTile == null)
        {
            Debug.LogError("Player's current tile is null. Cannot roll and move.");
            EnableBoardButtons(); // Re-enable buttons if state is broken.
            return;
        }

        //If the player somehow rolls 0 or less, something has probably gone wrong
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
        Player.Instance.SetCurrentBoardTile(finalTile); // Update logical position

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

        DisableBoardButtons();

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

                //Set the exp to be granted by the NCE from the SO
                encounterScript.SetExperienceGranted(encounter.grantedExperience);

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

    public void UpdateEncounterData(EncounterData encounterData)
    {
        if (Player.Instance == null)
        {
            Debug.LogError("Cannot update encounter data: Player does not exist");
            return;
        }

        int index = Player.Instance.GetTileIndex(); // Assumes player has GetTileIndex()
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
