using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Encounter : MonoBehaviour
{
    //No idea why i am making a parent class but I'm sure there will be a reason

    [SerializeField] private int experience; //experience to grant the player

    [SerializeField] private int coins; //coins to grant the player

    public int getExperience()
    {
        return experience;
    }

    public int getCoins()
    {
        return coins;
    }
}
