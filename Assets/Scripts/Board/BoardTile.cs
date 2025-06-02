using UnityEditor;
using UnityEngine;

public class BoardTile : MonoBehaviour
{
    [SerializeField] public BoardTile nextTile; // Single outgoing connection
    [SerializeField] private BoardTile prevTile; // Single incoming connection

    [SerializeField] private EncounterData encounter; // Encounter storage
    private bool isEncounterUntriggered = true;

    [SerializeField] private int index;

    public Transform playerPlacement;
    public Transform enemyPlacement;
    public Transform pathVisualiser;

    public bool isFirstorLastTile = false;

    // Accessor for the next tile
    public BoardTile GetNextTile()
    {
        return nextTile;
    }

    // Accessor for the previous tile
    public BoardTile GetPrevTile()
    {
        return prevTile;
    }

    // Method to set the previous tile reference, likely used by an editor script or board setup logic
    public void SetPreviousTile(BoardTile tile)
    {
        // This will overwrite any existing prevTile.
        // If 'tile' is null, it effectively clears the previous tile link.
        this.prevTile = tile;
    }

    // Method to explicitly clear the previous tile reference
    public void ClearPreviousTile()
    {
        this.prevTile = null;
    }

    // Method to establish a two-way link. Call this on one tile, and it will set up both forward and backward links.
    // Example: tileA.SetNextTileAndLinkBack(tileB);
    // This makes tileA.nextTile = tileB AND tileB.prevTile = tileA
    public void SetNextTileAndLinkBack(BoardTile newNextTile)
    {
        // Clear existing links if necessary
        if (this.nextTile != null)
        {
            this.nextTile.prevTile = null; // Old next tile no longer points back to this
        }
        if (newNextTile != null && newNextTile.prevTile != null)
        {
            newNextTile.prevTile.nextTile = null; // Tile that newNextTile pointed back to, no longer points to newNextTile
        }

        this.nextTile = newNextTile;
        if (newNextTile != null)
        {
            newNextTile.SetPreviousTile(this);
        }
    }


    public EncounterData GetEncounter()
    {
        return encounter;
    }

    public void SetIndex(int index) // Consider renaming to SetIndex for C# naming conventions
    {
        this.index = index;
    }

    public int GetIndex() // Consider renaming to GetIndex
    {
        return index;
    }

    public void SetEncounterData(EncounterData data)
    {
        encounter = data;
    }

    public void ClearEncounterVisual()
    {
        int count = 100;
        while (enemyPlacement.childCount > 0)
        {
            count--;
            Undo.DestroyObjectImmediate(enemyPlacement.GetChild(0).gameObject);

            // sanity escape hatch
            if (count == 0)
            {
                break;
            }
        }
    }

    public void GenerateEncounterVisual()
    {
        if (encounter == null) return;

        float scale = 0.5f;


        GameObject model = Instantiate(encounter.boardVisual, enemyPlacement.position, enemyPlacement.rotation, enemyPlacement);
        model.transform.localScale = new Vector3(scale, scale, scale);
    }
}
