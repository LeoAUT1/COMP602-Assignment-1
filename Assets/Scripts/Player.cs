using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : CombatEntity
{
    [SerializeField] protected int coins;
    [SerializeField] protected CameraController cam;
    [SerializeField] protected int indexOnGameBoard;
    private BoardTile currentTile;

    private void Start()
    {
        if (cam == null)
        {
            cam = FindAnyObjectByType<CameraController>(); // Good enough :^)
        }
        cam.target = this.transform;

        //We don't want to destroy the player on scene transition
        DontDestroyOnLoad(this);
    }

    public void AddCoins(int amount)
    {
        coins += amount;
    }

    public void SubtractCoins(int amount)
    {
        coins -= amount;
    }

    public int GetCoins() {
        return coins;
    }

    public BoardTile GetCurrentBoardTile()
    {
        return currentTile;
    }

    public void SetCurrentBoardTile(BoardTile tile)
    {
        currentTile = tile;
    }
}
