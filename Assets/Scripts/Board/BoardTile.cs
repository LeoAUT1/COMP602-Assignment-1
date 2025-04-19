using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class BoardTile : MonoBehaviour
{
    [SerializeField] public List<BoardTile> nextTiles; // Outgoing connections
    [SerializeField] private EncounterData encounter; // Encounter storage
    private bool isEncounterUntriggered = true;

    [SerializeField] private int index;

    public Transform playerPlacement;
    public Transform enemyPlacement;
    public Transform pathVisualiser;
    public BoardTile GetNextTile(int pathChoice = 0)
    {
        return pathChoice < nextTiles.Count ? nextTiles[pathChoice] : null;
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
