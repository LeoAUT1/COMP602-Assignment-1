using System.Collections;
using System.Collections.Generic;

public enum TargetType
{
    Self,
    Enemy
}

public abstract class AbilityBase
{
    public abstract string AbilityName { get; }
    public abstract string Description { get; }
    public abstract TargetType TargetType { get; }
    public abstract int Cooldown { get; }
    public abstract int CurrentCooldown { get; set; }
    public abstract IEnumerator Execute(CombatEntity caster, CombatEntity target, CombatManager combatManager, CombatHud hud);
    
    /// <returns>True if the ability can be used, false otherwise.</returns>
    public virtual bool CanUse(CombatEntity caster, CombatEntity primaryTarget, List<CombatEntity> allPotentialTargets)
    {
        // An ability can be used if its current cooldown is zero.
        // with any cooldown from ever being usable after the first time.
        if (CurrentCooldown > 0)
        {
            // Debug.LogWarning($"{AbilityName} is on cooldown: {CurrentCooldown} remaining.");
            return false;
        }
        return true;
    }
    public virtual void DecrementCooldown()
    {
        if (CurrentCooldown > 0)
        {
            CurrentCooldown--;
        }
    }
    public virtual void PutOnCooldown()
    {
        CurrentCooldown = Cooldown;
    }
}
