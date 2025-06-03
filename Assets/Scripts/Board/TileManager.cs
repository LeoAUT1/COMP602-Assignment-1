using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    // Start is called before the first frame update

    private BoardTile[] tiles;

    [SerializeField] private float tileVisualScale = 0.25f;

    void Start()
    {
        //Abort early if the Game manager has encounte randomisation disabled
        if (!GameManager.Instance.enableEncounterRandomisation) { return; }


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
            if (tile.isFirstorLastTile) continue;

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
            if (tile.isFirstorLastTile == true)
            {
                //Pass on the first and last tile
                continue;
            }

            if (encounters[count] == null)
            {
                count++;
                continue;
            }

            //Debug.Log($"Setting {tile} to {encounters[count].name}");

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
            if (tile.isFirstorLastTile) continue;

            tilesProcessed++;
            EncounterData encounterBase = tile.GetEncounter();

            if (encounterBase != null && encounterBase.boardVisual != null)
            {
                GameObject go = Instantiate(encounterBase.boardVisual, tile.enemyPlacement.position, Quaternion.identity, tile.enemyPlacement);
                go.transform.localScale = new Vector3(tileVisualScale, tileVisualScale, tileVisualScale);
            }
        }

        Debug.Log($"Processed {tilesProcessed} tiles. Spawned {enemiesActuallySpawned} enemy visuals.");
    }
}
