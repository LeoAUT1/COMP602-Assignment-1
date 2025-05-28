using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PowerupData", menuName = "Game/Powerup")]

public class PowerupData : ScriptableObject
{
    public string powerupName;
    public string description;
    public Sprite icon;

    public int count = 1; //Number of times the player has the effect applied to theme, by default one.

    [Header("StatusEffect Effect (if applicable)")]
    public StatusEffect statusEffect; //This should be applied to the player on combat start

    public void Increment()
    {
        count += 1;
    }

}
