using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardTile : MonoBehaviour
{
    [SerializeField] public List<BoardTile> nextTiles; // Outgoing connections
    //[SerializeField] private TileType tileType; // Gameplay properties

    // Methods for gameplay logic
    public BoardTile GetNextTile(int pathChoice = 0)
    {
        return pathChoice < nextTiles.Count ? nextTiles[pathChoice] : null;
    }
}
