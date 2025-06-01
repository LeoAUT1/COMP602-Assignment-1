using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class PlayerCombat : CombatEntity
{

    // Example properties for ICombatUnit
    public string GetUnitName() { return Player.Instance.playerName; } // Or some other name field

    private Dictionary<string, PowerupData> powerUps = new Dictionary<string, PowerupData>();

    public override void Initialise(CombatManager cm, CombatHud hud)
    {
        base.Initialise(cm, hud);

        // Grant the player the basic ability
        AddAbility(new BasicAttackAbility());
        //AddAbility(new DoTAttack());

        foreach (PowerupData pup in powerUps.Values)
        {
            AddStatusEffect(pup.statusEffect);
        }
    }

    public Dictionary<string, PowerupData> GetPowerUps()
    {
        return powerUps;
    }

    public void AddPowerup(string name, PowerupData powerup)
    {
        Debug.Log($"Adding {powerup} to PlayerCombat");
        if (powerUps.TryGetValue(powerup.powerupName, out PowerupData existingPowerup))
        {
            // Powerup already exists, increment its value
            existingPowerup.Increment();
            return;
        }

        powerUps.Add(powerup.powerupName, powerup);
    }

    //new abilites when leveling up
    public void LearnAbility(int level)
    {
        AbilityBase newAbility = null;

        switch (level)
        {
            case 2:
                newAbility = new DoubleSlash();
                break;
            case 3:
                newAbility = new DrainLife();
                break;
            case 4:
                newAbility = new BurningBlade();
                break;
            case 5:
                newAbility = new HeavyAttack();
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

    private int GetCountOfSkillPassive(string name)
    {
        int count = 0;
        foreach (PowerupData powerup in powerUps.Values)
        {
            if (powerup.powerupName == name)
            {
                count++;
            }
        }
        return count;
    }

    private int SkillMultiplier(int skillLevel, int count)
    {
        return (int)(skillLevel + count * (1 + count / 10.0f));
    }

    // Hard coding the names like this is bad.

    public override int GetIntelligence()
    {
        int count = GetCountOfSkillPassive("Magi's Mind");
        return SkillMultiplier(intelligence, count);
    }

    public override int GetDexterity()
    {
        int count = GetCountOfSkillPassive("Trickster's Art");
        return SkillMultiplier(dexterity, count);
    }

    public override int GetStrength()
    {
        int count = GetCountOfSkillPassive("Giant's Strength");
        return SkillMultiplier(strength, count);
    }
}
