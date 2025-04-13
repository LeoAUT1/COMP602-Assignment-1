using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardTile : MonoBehaviour, IClickable
{
    [SerializeField] public List<BoardTile> nextTiles; // Outgoing connections
    [SerializeField] private Encounter encounter; // Gameplay properties

    // Methods for gameplay logic
    public BoardTile GetNextTile(int pathChoice = 0)
    {
        return pathChoice < nextTiles.Count ? nextTiles[pathChoice] : null;
    }

    // Implementation of IClickable interface
    public void OnClick()
    {

        Debug.Log($"Tile clicked: {gameObject.name}");

        if (encounter != null)
        {
            // encounter.Trigger();
        }
    }
}
