using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    // Start is called before the first frame update

    private BoardTile[] tiles;

    void Start()
    {
        tiles = GetComponentsInChildren<BoardTile>();

        if (tiles == null || tiles.Length == 0)
        {
            Debug.LogWarning($"No {nameof(BoardTile)} components found as children of {name}.");
        }

        if (GameManager.Instance == null)
        {
            Debug.Log("Cant find game manager");

        }

        int numToRequest = tiles.Length - 2;

        Debug.Log($"requesting {numToRequest} encounters from the game manager");

        EncounterData[] encountersToWrite = GameManager.Instance.GetEncounters(numToRequest);
        ClearEncounters();
        SetEncounters(encountersToWrite);
        GenerateTileVisuals();
    }



    public void ClearEncounters()
    {
        foreach (BoardTile tile in tiles)
        {
            if (tile.ToString().Contains("BossTile")) continue;

            //Debug.Log($"Clearing encounter from: {tile.name}");
            tile.SetEncounterData(null);
        }
    }

    public void SetEncounters(EncounterData[] encounters)
    {

        Debug.Log($"setting {encounters.Length} encounters");
        int count = 0;
        foreach (BoardTile tile in tiles)
        {
            if (tile.name == "StartTile" || tile.name == "BossTile")
            {
                //Pass on the first and last tile
                continue;
            }

            Debug.Log(tile);
            Debug.Log($"Setting {tile} to {encounters[count].name}");

            tile.SetEncounterData(encounters[count]);

            count++;
        }
    }

    private void GenerateTileVisuals()
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
