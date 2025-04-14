using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Encounter : MonoBehaviour
{
    //No idea why i am making a parent class but I'm sure there will be a reason

    [SerializeField] private int experience; //experience to grant the player
    [SerializeField] private int coins; //coins to grant the player
    private bool isEncounterFinished = false;

    public int GetExperience()
    {
        return experience;
    }

    public int GetCoins()
    {
        return coins;
    }

    public bool GetIsEncounterFinished()
    {
        return isEncounterFinished;
    }
}
