using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCombat : CombatEntity
{

    // Example properties for ICombatUnit
    public string GetUnitName() { return Player.Instance.playerName; } // Or some other name field

    private Dictionary<string, PowerupData> powerUps = new Dictionary<string, PowerupData>();

    [SerializeField] private RegenPassive regenEffectTemplate;

    public override void Initialise(CombatManager cm, CombatHud hud)
    {
        base.Initialise(cm, hud);
        // Grant the player the basic ability
        AddAbility(new BasicAttackAbility());

        AddStatusEffect(regenEffectTemplate);
    }

    public Dictionary<string, PowerupData> GetPowerUps()
    {
        return powerUps;
    }

    public void AddPowerup(string name, PowerupData powerup)
    {

        if (powerUps.TryGetValue(powerup.name, out PowerupData existingPowerup))
        {
            // Powerup already exists, increment its value
            existingPowerup.Increment();
            return;
        }

        powerUps.Add(name, powerup);
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
