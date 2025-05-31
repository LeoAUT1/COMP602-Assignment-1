using System;
using UnityEngine;

public class RandomiseEncounters : MonoBehaviour
{
    [SerializeField] private EncounterData[] encounters;
    private BoardTile[] tiles;

    [SerializeField] private GameObject tileContainer;

    public void Start()
    {
        Debug.Log("RandomiseEncounters.Start() running");

        if (tileContainer == null)
        {
            Debug.LogError($"{nameof(tileContainer)} is not assigned in the Inspector for {gameObject.name}. This script cannot proceed.");
            enabled = false;
            return;
        }

        tiles = tileContainer.GetComponentsInChildren<BoardTile>();
        if (tiles == null || tiles.Length == 0)
        {
            Debug.LogWarning($"No {nameof(BoardTile)} components found as children of {tileContainer.name}.");
        }

        if (encounters == null || encounters.Length == 0)
        {
            Debug.LogWarning($"{nameof(encounters)} array is null or empty.");
        }

        ClearEncounters();
        RandomiseEncounter();
        GenerateVisualsAtRuntime();
    }

    public void RandomiseEncounter()
    {
        if (encounters == null || encounters.Length == 0)
        {
            Debug.LogWarning("Encounters array is null or empty. Skipping randomisation.");
            return;
        }

        ArrayShuffleUtility.Shuffle(encounters);

        int count = 0;
        foreach (BoardTile tile in tiles)
        {
            if (tile.ToString().Contains("BossTile")) continue;

            EncounterData encounterToAssign = encounters[count];
            tile.SetEncounterData(encounterToAssign);
            Debug.Log($"Assigning {encounterToAssign.encounterName} to tile: {tile.name}");

            count++;
            if (count >= encounters.Length)
            {
                count = 0;
                ArrayShuffleUtility.Shuffle(encounters);
            }
        }
    }

    public void ClearEncounters()
    {
        foreach (BoardTile tile in tiles)
        {
            if (tile.ToString().Contains("BossTile")) continue;

            Debug.Log($"Clearing encounter from: {tile.name}");
            tile.SetEncounterData(null);
        }
    }

    private void GenerateVisualsAtRuntime()
    {
        int enemiesActuallySpawned = 0;
        int tilesProcessed = 0;

        Debug.Log("Clearing existing enemy visuals...");
        foreach (BoardTile tile in tiles)
        {
            if (tile.enemyPlacement == null)
            {
                Debug.LogWarning($"Tile {tile.name} has no enemyPlacement.");
                continue;
            }

            foreach (Transform child in tile.enemyPlacement)
            {
                Destroy(child.gameObject);
            }
        }

        Debug.Log("Spawning new visuals...");
        foreach (BoardTile tile in tiles)
        {
            tilesProcessed++;
            EncounterData encounterBase = tile.GetEncounter();

            if (encounterBase != null && encounterBase is EncounterData encounterData)
            {
                if (encounterData.enemies != null && encounterData.enemies.Length > 0)
                {
                    foreach (Enemy enemyPrefab in encounterData.enemies)
                    {
                        if (enemyPrefab == null || tile.enemyPlacement == null) continue;

                        Debug.Log($"Spawning {enemyPrefab.name} on tile {tile.name}");
                        Enemy modelInstance = Instantiate(enemyPrefab, tile.enemyPlacement.position, Quaternion.identity, tile.enemyPlacement);
                        modelInstance.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

                        enemiesActuallySpawned++;
                    }
                }
                else if (encounterData.boardVisual != null)
                {
                    // Spawn priest or trap visuals etc.
                    Instantiate(encounterData.boardVisual, tile.enemyPlacement.position, Quaternion.identity, tile.enemyPlacement);
                }
            }
        }

        Debug.Log($"Processed {tilesProcessed} tiles. Spawned {enemiesActuallySpawned} enemy visuals.");
    }
}

public static class ArrayShuffleUtility
{
    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(T[] array)
    {
        if (array == null) throw new ArgumentNullException(nameof(array));

        int n = array.Length;
        for (int i = n - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            T temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }
}
