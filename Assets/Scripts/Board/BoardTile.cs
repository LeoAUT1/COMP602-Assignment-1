using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class BoardTile : MonoBehaviour
{
    [SerializeField] public List<BoardTile> nextTiles = new List<BoardTile>(); // Outgoing connections (Initialized)
    private List<BoardTile> prevTiles = new List<BoardTile>(); // Incoming connections (Initialized)

    [SerializeField] private EncounterData encounter; // Encounter storage
    private bool isEncounterUntriggered = true;

    [SerializeField] private int index;

    public Transform playerPlacement;
    public Transform enemyPlacement;
    public Transform pathVisualiser;

    public IReadOnlyList<BoardTile> PreviousTiles => prevTiles?.AsReadOnly();

    public BoardTile GetNextTile(int pathChoice = 0)
    {
        return (nextTiles != null && pathChoice >= 0 && pathChoice < nextTiles.Count) ? nextTiles[pathChoice] : null;
    }

    public BoardTile GetPrevTile(int pathChoice = 0)
    {
        // Ensure prevTiles is initialized before accessing
        if (prevTiles == null) prevTiles = new List<BoardTile>();

        return (pathChoice >= 0 && pathChoice < prevTiles.Count) ? prevTiles[pathChoice] : null;
    }

    // Method for the editor script to add a previous tile reference
    public void AddPreviousTile(BoardTile tile)
    {
        if (prevTiles == null) prevTiles = new List<BoardTile>(); // Defensive initialization

        if (tile != null && !prevTiles.Contains(tile)) // Prevent nulls and duplicates
        {
            prevTiles.Add(tile);
        }
    }

    // Method for the editor script to clear previous tiles before recalculating
    public void ClearPreviousTiles()
    {
        if (prevTiles != null)
        {
            prevTiles.Clear();
        }
        else
        {
            prevTiles = new List<BoardTile>(); // Ensure it's initialized
        }
    }

    public EncounterData GetEncounter()
    {
        return encounter;
    }

    public void setIndex(int index)
    {
        this.index = index;
    }

    public int getIndex()
    {
        return index;
    }

    public void SetEncounterData(EncounterData data)
    {
        encounter = data;
    }
}
