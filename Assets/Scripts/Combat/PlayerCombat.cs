using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerCombat : CombatEntity
{

    public event Action OnStatsChanged; // Event for triggering player UI to update

    [Header("Player's Starting Stats")]
    // Use these to set and reset the player's combat stats
    [SerializeField] private int initialHP;
    [SerializeField] private int initialStrength;
    [SerializeField] private int initialIntelligence;
    [SerializeField] private int initialDexterity;
    [SerializeField] private int healthPerLevel = 10;

    // Example properties for ICombatUnit
    public string GetUnitName() { return Player.Instance.playerName; } // Or some other name field

    private Dictionary<string, PowerupData> powerUps = new Dictionary<string, PowerupData>();

    public void Start()
    {
        ResetPlayerCombat();
        OnStatsChanged?.Invoke();

        // Grant the player the basic attack ability
        //AddAbility(new BasicAttackAbility());
    }

    public override void Initialise(CombatManager cm, CombatHud hud, CombatEntity enemy)
    {
        base.Initialise(cm, hud, enemy);

        //Set the player's stats to our starting stats
        ClearAllStatusEffects();

        foreach (PowerupData pup in powerUps.Values)
        {
            Debug.Log(pup.count);
            Debug.Log(pup.name);
            Debug.Log(pup.statusEffect);
            AddStatusEffect(pup.statusEffect, pup.count, enemy);
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
    public AbilityBase LearnAbility(int level)
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
                return null;
        }

        if (newAbility != null)
        {
            AddAbility(newAbility);
            Debug.Log($"{GetName()} learned new ability: {newAbility.AbilityName}");
        }

        //New ability gets returned so its details can be displayed on the levelup canvas
        return newAbility;

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

    // Run this when the player starts a new game to guarantee their stats are reset
    public void ResetPlayerCombat()
    {
        SetHealth(initialHP);
        SetMaxHealth(initialHP);
        SetStrength(initialStrength);
        SetDexterity(initialDexterity);
        SetIntelligence(initialIntelligence);

        //player is alive when hp is above 0
        SetAlive(true);

        //Clear all the player's powerups
        powerUps = new Dictionary<string, PowerupData>();

        //Clear ability list
        ResetAllAbilities();

        //Clear any status effects
        ClearAllStatusEffects();

        OnStatsChanged?.Invoke();
    }

    public void AddStrength(int amount)
    {
        SetMaxHealth(GetMaxHealth() + healthPerLevel);
        AddHealth(healthPerLevel);
        strength += amount;
        OnStatsChanged?.Invoke();
    }

    public void AddDexterity(int amount)
    {
        SetMaxHealth(GetMaxHealth() + healthPerLevel);
        AddHealth(healthPerLevel);
        dexterity += amount;
        OnStatsChanged?.Invoke();
    }

    public void AddIntelligence(int amount)
    {
        SetMaxHealth(GetMaxHealth() + healthPerLevel);
        AddHealth(healthPerLevel);
        intelligence += amount;
        OnStatsChanged?.Invoke(); 
    }
}
