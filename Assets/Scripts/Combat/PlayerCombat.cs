using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : CombatEntity
{

    // Example properties for ICombatUnit
    public string GetUnitName() { return Player.Instance.playerName; } // Or some other name field


    // Example of initializing abilities:
    void Start() // Or Awake, or a specific Init method called after Player.Instance is ready
    {
        // Ensure entityName is set if you're not overriding GetName()
        // this.entityName = Player.Instance.playerName;

        AddAbility(new BasicAttackAbility());

        AddAbility(new TestAbility());
        // Add other abilities the player starts with
    }
}
