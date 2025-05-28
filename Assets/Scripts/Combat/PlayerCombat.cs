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

        //base ability/skill
        AddAbility(new BasicAttackAbility());
        
        // Add other abilities the player starts with
    }
    
    //new abilites when leveling up
    public void LearnAbility(int level)
    {
        AbilityBase newAbility = null;

        switch (level)
        {
            case 2:
                newAbility = new HeavyAttack();
                break;
            case 3:
                newAbility = new TestAbility();
                break;
            default:
                Debug.Log("no new ability");
                 return;
        }

        if (newAbility != null)
        {
            AddAbility(newAbility);
            Debug.Log($"{GetName()} learned new ability: {newAbility.AbilityName}");
        }

    }
}
