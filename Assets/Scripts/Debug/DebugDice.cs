using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DebugDice : MonoBehaviour
{

    public Transform spawnHere;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RollDice()
    {
        StartCoroutine(DiceManager.Instance.RollTheDiceCoroutine(
            (diceRollResult) => {
                Debug.Log($"Player rolled: {diceRollResult}");
                RolledDice(diceRollResult); // Board calls its own method
            },
            spawnHere.position // Pass the calculated spawn position
        ));
    }

    private void RolledDice(int res)
    {
        Debug.Log($"Rolled dice for {res}");
    }
}
