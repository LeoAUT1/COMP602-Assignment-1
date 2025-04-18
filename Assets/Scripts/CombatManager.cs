using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{

    private Player player;
    private Enemy[] enemies;
    private EncounterData encounter;

    public void InitialiseCombat(Player player, EncounterData encounter)
    {
        // Do stuff here to set up whatever is necessary for the combat scene 
        Instantiate(encounter.enemies[0]);

        this.encounter = encounter;
        this.player = player;
        this.enemies = encounter.enemies;
    }

    public void ExitCombat()
    {

        //Grant player the reward for completing the encounter
        player.AddExperience(encounter.grantedExperience);
        player.AddCoins(encounter.grantedCoins);

        //back to the Board Scene
        GameManager.Instance.ExitCombat();
    }
}
