using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{

    private Player player;
    private Enemy[] enemies;

    public void InitialiseCombat(Player player, EncounterData encounter)
    {
        // Do stuff here to set up whatever is necessary for the combat scene 
        Instantiate(encounter.enemies[0]);

        this.player = player;
        this.enemies = encounter.enemies;
    }

    public void ExitCombat()
    {
        //Combat has finished, back to the Board Scene
        GameManager.Instance.ExitCombat();
    }
}
