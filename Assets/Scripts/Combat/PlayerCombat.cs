using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : CombatEntity
{
    private List<StatusEffect> activeStatusEffects = new List<StatusEffect>();

    // Example properties for ICombatUnit
    public string GetUnitName() { return Player.Instance.playerName; } // Or some other name field

    public void Heal(int amount)
    {
        AddHealth(amount);
    }
}
