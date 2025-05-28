using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCombat : CombatEntity
{

    // Example properties for ICombatUnit
    public string GetUnitName() { return Player.Instance.playerName; } // Or some other name field

    private Dictionary<string, PowerupData> powerUps = new Dictionary<string, PowerupData>();

    void Awake()
    {
        Initialise();
    }

    private void Initialise()
    {
        // Grant the player the basic ability
        AddAbility(new BasicAttackAbility());
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
}
