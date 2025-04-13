using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : CombatEntity
{
    [SerializeField] protected int coins;
    [SerializeField] protected CameraController cam;
    [SerializeField] protected int indexOnGameBoard;

    private void Start()
    {
        if (cam == null)
        {
            cam = FindAnyObjectByType<CameraController>(); // Good enough :^)
            cam.target = this.transform;
        }
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
}
