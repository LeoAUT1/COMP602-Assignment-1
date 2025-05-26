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
            tile.SetIndex(id++);
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
            tile.ClearEncounterVisual();
        }
        Debug.Log("Previous visuals cleared.");

        // Now, generate new visuals
        Debug.Log("Generating new visuals...");
        foreach (BoardTile tile in tiles)
        {
            tile.GenerateEncounterVisual();
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


    private void PropagateConnections(Board board) 
    {
        BoardTile[] tiles = GetAllTiles(board); // You'll need to implement or provide GetAllTiles
        if (tiles == null || tiles.Length == 0)
        {
            Debug.Log("No tiles found to propagate connections.");
            return;
        }

        Debug.Log("Propagating back connections for single forward/backward links...");

        foreach (BoardTile tile in tiles)
        {
            if (tile.GetPrevTile() != null) // Only act if there's a previous tile to clear
            {
                Undo.RecordObject(tile, "Clear Previous Tile Link");
                tile.SetPreviousTile(null); // Use the new setter, or ClearPreviousTile()
                EditorUtility.SetDirty(tile); // Mark this tile as dirty as its prevTile changed
            }
        }
        Debug.Log("Cleared existing previous tile links.");

        // 2. Iterate through each tile and set the 'prevTile' of its 'nextTile'
        int connectionsMade = 0;
        foreach (BoardTile currentTile in tiles)
        {
            BoardTile nextTileInstance = currentTile.GetNextTile(); // Or access currentTile.nextTile directly

            if (nextTileInstance != null)
            {
                // Record 'nextTileInstance' because we are modifying its prevTile
                Undo.RecordObject(nextTileInstance, "Set Previous Tile Link");

                // Check if this nextTileInstance is already pointed to by another prevTile.
                // This can happen if multiple tiles have nextTileInstance as their nextTile (a merge point).
                // With a single prevTile, the last one processed will "win".
                if (nextTileInstance.GetPrevTile() != null && nextTileInstance.GetPrevTile() != currentTile)
                {
                    Debug.LogWarning($"Tile '{nextTileInstance.name}' (Index: {nextTileInstance.GetIndex()}) " +
                                     $"already had a previous tile: '{nextTileInstance.GetPrevTile().name}' (Index: {nextTileInstance.GetPrevTile().GetIndex()}). " +
                                     $"It will now be linked from '{currentTile.name}' (Index: {currentTile.GetIndex()}). " +
                                     "This implies a merge point where the last processed link takes precedence for 'prevTile'.", nextTileInstance);
                }

                nextTileInstance.SetPreviousTile(currentTile);
                EditorUtility.SetDirty(nextTileInstance); // Mark the modified tile as dirty
                connectionsMade++;
            }
            // If currentTile.nextTile is null, there's no forward connection, so nothing to propagate backward.
        }

        Debug.Log($"Propagated {connectionsMade} back-connections across {tiles.Length} tiles.");

        // Save the scene changes if any connections were made and we're in the editor
        if (connectionsMade > 0 && !Application.isPlaying)
        {
            if (board != null && board.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(board.gameObject.scene);
            }
            else if (board == null)
            {
                Debug.LogWarning("Board object is null. Cannot mark scene dirty.");
            }
            else
            {
                Debug.LogWarning("Board's game object scene is not valid. Cannot mark scene dirty. This might happen if 'board' is a prefab asset not instantiated in a scene, or the scene is not loaded.");
            }
        }
    }
}
