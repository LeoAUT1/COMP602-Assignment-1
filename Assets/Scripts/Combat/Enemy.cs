using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Implementation example class
public class Enemy : CombatEntity
{
    [SerializeField] protected int experienceGranted; //experience given to the player when they defeat this enemy

    public int getExperience() { return experienceGranted; } //Not sure if we need this
}