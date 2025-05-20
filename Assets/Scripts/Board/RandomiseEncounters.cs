using System;
using UnityEngine;

public class RandomiseEncounters : MonoBehaviour
{
    //All the possible encounters we want to distribute over the tiles we have in the scene
    [SerializeField] private EncounterData[] encounters;
    private BoardTile[] tiles;

    [SerializeField] private GameObject tileContainer;

    public void Start()
    {
        if (tileContainer == null)
        {
            Debug.LogError($"{nameof(tileContainer)} is not assigned in the Inspector for {gameObject.name}. This script cannot proceed.");
            enabled = false; // Disable this component to prevent further errors.
            return;
        }

        tiles = tileContainer.GetComponentsInChildren<BoardTile>();
        if (tiles == null || tiles.Length == 0)
        {
            Debug.LogWarning($"No {nameof(BoardTile)} components found as children of {tileContainer.name}. Encounter randomisation might not work as intended.");
            // Depending on your game logic, you might want to return or disable here too.
        }

        if (encounters == null || encounters.Length == 0)
        {
            Debug.LogWarning($"{nameof(encounters)} array is null or empty in the Inspector for {gameObject.name}. No encounters can be assigned.");
            // If encounters are essential, consider disabling or returning.
            // For RandomiseEncounter, it should handle this gracefully.
        }

        ClearEncounters();
        RandomiseEncounter();
        GenerateVisualsAtRuntime();
    }

    public void RandomiseEncounter()
    {
        if (encounters == null || encounters.Length == 0)
        {
            Debug.LogWarning("Encounters array is null or empty. Skipping encounter randomisation.");
            return; // Crucial: Exit if there's nothing to distribute.
        }

        //Shuffle our encounters
        ArrayShuffleUtility.Shuffle(encounters);

        int count = 0;
        foreach (BoardTile tile in tiles) // Ensure 'tiles' is not null if you reach here
        {
            if (tile.ToString().Contains("BossTile")) // Still a bit fragile, but not the crash cause
            {
                continue;
            }

            tile.SetEncounterData(encounters[count]);

            count++;
            if (count >= encounters.Length)
            {
                count = 0;


                //Shuffle our encounters, again
                ArrayShuffleUtility.Shuffle(encounters);
            }
        }
    }

    public void ClearEncounters()
    {

        foreach (BoardTile tile in tiles)
        {
            if (tile.ToString().Contains("BossTile"))
            {
                continue;
            }

            Debug.Log(tile.ToString());

            tile.SetEncounterData(null);
        }

    }

    private void GenerateVisualsAtRuntime() // Renamed for clarity, if you wish
    {
        int enemiesActuallySpawned = 0; // Renamed for clarity and will be correctly incremented
        int tilesProcessed = 0;

        // First, clear existing enemy visuals
        Debug.Log("Clearing previous visuals...");
        foreach (BoardTile tile in tiles)
        {
            if (tile.enemyPlacement == null)
            {
                Debug.LogWarning($"Tile {tile.name} has no enemyPlacement assigned. Skipping cleanup for this tile.");
                continue;
            }

            foreach (Transform child in tile.enemyPlacement)
            {
                Destroy(child.gameObject);
            }

        }
        Debug.Log("Previous visuals cleared.");

        // Now, generate new visuals
        Debug.Log("Generating new visuals...");
        foreach (BoardTile tile in tiles)
        {
            tilesProcessed++;
            EncounterData encounterBase = tile.GetEncounter(); // Assuming GetEncounter() is your method

            if (encounterBase != null && encounterBase is EncounterData encounterData) // Using 'is' for type check and cast
            {
                if (encounterData.enemies != null && encounterData.enemies.Length > 0)
                {
                    foreach (Enemy enemyPrefab in encounterData.enemies) // Assuming 'enemyData' is actually a prefab
                    {
                        if (enemyPrefab == null)
                        {
                            Debug.LogWarning($"Encounter on tile {tile.name} has a null enemy prefab entry. Skipping this enemy.");
                            continue;
                        }

                        if (tile.enemyPlacement == null)
                        {
                            Debug.LogError($"Cannot spawn enemy '{enemyPrefab.name}' on tile {tile.name} because enemyPlacement is null.");
                            continue;
                        }

                        Debug.Log($"Spawning {enemyPrefab.name} on tile {tile.name}");

                        // Instantiate the enemy prefab
                        Enemy modelInstance = Instantiate(enemyPrefab, tile.enemyPlacement.position, Quaternion.identity, tile.enemyPlacement);
                        modelInstance.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                        // Consider if Quaternion.Euler(0,0,0) was intentional or if Quaternion.identity is more appropriate.
                        // For 2D, Euler(0,0,0) is fine. For 3D, identity is often a better default unless a specific rotation is needed.

                        enemiesActuallySpawned++; // Increment for each enemy instantiated
                    }
                }
            }
        }

        Debug.Log($"Processed {tilesProcessed} tiles. Spawned {enemiesActuallySpawned} enemy visuals.");
    }

}

//Helper shuffle class
public static class ArrayShuffleUtility
{
    // A single Random instance can be reused for better randomness
    // compared to creating a new one each time, especially if called rapidly.
    private static System.Random rng = new System.Random();
    public static void Shuffle<T>(T[] array)
    {
        if (array == null)
        {
            // Or handle as you see fit, perhaps log a warning.
            // Throwing an exception is standard for invalid arguments.
            throw new ArgumentNullException(nameof(array));
        }

        int n = array.Length;
        for (int i = n - 1; i > 0; i--)
        {
            // Pick a random index j from the remaining unshuffled elements (0 to i, inclusive).
            int j = rng.Next(0, i + 1); // Next(minValue, maxValue) - maxValue is exclusive, so i + 1.

            // Swap array[i] with array[j]
            T temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }
}