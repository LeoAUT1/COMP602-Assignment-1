using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using System.Linq;

[CustomEditor(typeof(Board))]
public class BoardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Board board = (Board)target;


        if (GUILayout.Button("Assign Tile IDs"))
        {
            AssignTileIDs(board);
        }

        if (GUILayout.Button("Generate Tile Visuals"))
        {
            GenerateVisuals(board);
        }

        if (GUILayout.Button("Propagate Back Connections"))
        {
            PropagateConnections(board);
        }
    }

    private void AssignTileIDs(Board board)
    {
        // Get the tile container reference via SerializedProperty
        SerializedProperty tileContainerProp = serializedObject.FindProperty("tileContainer");
        GameObject tileContainer = tileContainerProp.objectReferenceValue as GameObject;

        if (tileContainer == null)
        {
            Debug.LogError("Tile Container is not assigned!");
            return;
        }

        // Get all BoardTile components from children
        BoardTile[] tiles = tileContainer.GetComponentsInChildren<BoardTile>();

        int id = 0;
        foreach (BoardTile tile in tiles)
        {
            tile.setIndex(id++);
            EditorUtility.SetDirty(tile); // Mark the tile as dirty so changes are saved
        }

        Debug.Log($"Assigned IDs to {id} tiles");

        // Save the changes
        EditorUtility.SetDirty(board);
        if (!Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(board.gameObject.scene);
        }
    }

    private void GenerateVisuals(Board board)
    {
        // Get the tile container reference via SerializedProperty
        SerializedProperty tileContainerProp = serializedObject.FindProperty("tileContainer");

        if (tileContainerProp == null)
        {
            Debug.LogError("Board script does not have a 'tileContainer' property.");
            return;
        }
        GameObject tileContainer = tileContainerProp.objectReferenceValue as GameObject;

        if (tileContainer == null)
        {
            Debug.LogError("Tile Container is not assigned in the Board inspector!");
            return;
        }

        // Get all BoardTile components from children
        BoardTile[] tiles = tileContainer.GetComponentsInChildren<BoardTile>();

        int enemiesSpawned = 0;
        int tilesProcessed = 0;

        // First, clear existing enemy visuals to prevent duplicates
        // This assumes enemies are direct children and you want to remove *all* children
        // Modify this if other important children exist or if enemies are tagged differently
        Debug.Log("Clearing previous visuals...");
        foreach (BoardTile tile in tiles)
        {
            // DestroyImmediate is necessary in Editor scripts for objects not part of a prefab instance
            while (tile.enemyPlacement.childCount > 0)
            {
                Undo.DestroyObjectImmediate(tile.enemyPlacement.transform.GetChild(0).gameObject);
            }
        }
        Debug.Log("Previous visuals cleared.");

        // Now, generate new visuals
        Debug.Log("Generating new visuals...");
        foreach (BoardTile tile in tiles)
        {
            tilesProcessed++;
            // Assuming BoardTile has a public field 'encounter'
            // Replace 'encounter' with the actual field/property name if different
            EncounterData encounterBase = tile.GetEncounter();

            if (encounterBase != null && encounterBase is EncounterData encounterData)
            {
                // Check if the EncounterData has enemies
                // Replace 'enemies' with the actual field/property name if different
                if (encounterData.enemies != null && encounterData.enemies.Length > 0)
                {
                    foreach (Enemy enemyData in encounterData.enemies)
                    {
                        Debug.Log(enemyData.name);

                        Enemy model = Instantiate(enemyData, tile.enemyPlacement.transform.position,Quaternion.Euler(0,0,0), tile.enemyPlacement.transform);
                        model.transform.localScale = new Vector3(0.25f, .25f, .25f);
                    }
                }
            }
        }

        Debug.Log($"Processed {tilesProcessed} tiles. Spawned {enemiesSpawned} enemy visuals.");

        // Save the changes
        if (enemiesSpawned > 0) // Only mark dirty if changes were made
        {
            EditorUtility.SetDirty(board);
            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(board.gameObject.scene);
            }
        }
    }

    // Helper function to get all tiles (used by multiple methods)
    private BoardTile[] GetAllTiles(Board board)
    {
        SerializedProperty tileContainerProp = serializedObject.FindProperty("tileContainer");
        if (tileContainerProp == null)
        {
            Debug.LogError("Board script does not have a 'tileContainer' property.");
            return null;
        }
        GameObject tileContainer = tileContainerProp.objectReferenceValue as GameObject;

        if (tileContainer == null)
        {
            Debug.LogError("Tile Container is not assigned in the Board inspector!");
            return null;
        }

        return tileContainer.GetComponentsInChildren<BoardTile>();
    }

    // --- NEW: Function to propagate back connections ---
    private void PropagateConnections(Board board)
    {
        BoardTile[] tiles = GetAllTiles(board);
        if (tiles == null) return;

        Debug.Log("Propagating back connections...");

        // 1. Clear all existing previous tile references on all tiles
        foreach (BoardTile tile in tiles)
        {
            // Record object state before modification for Undo
            Undo.RecordObject(tile, "Clear Previous Tiles");
            tile.ClearPreviousTiles();
            // No need to SetDirty here yet, will do it after adding new ones
        }
        Debug.Log("Cleared existing previous tile links.");

        // 2. Iterate through each tile and add 'self' to the 'prevTiles' list of its 'nextTiles'
        int connectionsMade = 0;
        foreach (BoardTile currentTile in tiles)
        {
            if (currentTile.nextTiles == null) continue; // Skip if no next tiles defined

            foreach (BoardTile nextTile in currentTile.nextTiles)
            {
                if (nextTile != null)
                {
                    // Record the 'nextTile' because we are modifying its prevTiles list
                    Undo.RecordObject(nextTile, "Add Previous Tile Link");
                    nextTile.AddPreviousTile(currentTile);
                    EditorUtility.SetDirty(nextTile); // Mark the modified tile as dirty
                    connectionsMade++;
                }
                else
                {
                    Debug.LogWarning($"Tile {currentTile.getIndex()} has a null entry in its nextTiles list.", currentTile);
                }
            }
        }

        Debug.Log($"Propagated {connectionsMade} back-connections across {tiles.Length} tiles.");

        // Save the scene changes
        if (connectionsMade > 0 && !Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(board.gameObject.scene);
        }
    }
}
