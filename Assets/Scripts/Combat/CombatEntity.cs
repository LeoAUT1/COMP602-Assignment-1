using System.Collections.Generic;
using UnityEngine;

public abstract class CombatEntity : MonoBehaviour
{
    [SerializeField] protected string entityName;
    [SerializeField] protected int health;
    [SerializeField] protected int maxHealth;
    [SerializeField] protected int strength;
    [SerializeField] protected int dexterity;
    [SerializeField] protected int intelligence;
    private List<StatusEffect> statusEffects = new List<StatusEffect>();


    public List<AbilityBase> Abilities { get; protected set; } = new List<AbilityBase>();

    private CombatManager combatManagerInstance; // Cache this
    private CombatHud combatHud;

    private bool isAlive = true;

    public virtual void Initialise(CombatManager cm, CombatHud hud) // Call this from CombatManager.SetupBattle
    {
        this.combatManagerInstance = cm;
        this.combatHud = hud; // Or get it from cm
    }

    public virtual int SetHealth(int amount)
    {
        health = amount;
        return health;
    }

    public virtual int AddHealth(int amount)
    {
        health += amount;
        health = Mathf.Min(health, maxHealth); // Prevent exceeding max health

        return health;
    }

    public virtual int SubtractHealth(int amount)
    {
        health -= amount;
        health = Mathf.Max(health, 0); // Prevent falling below 0

        if (health == 0)
        {
            isAlive = false;
        }

        return health;
    }

    public void TakeDamage(int amount, CombatEntity attacker = null)
    {
        Debug.Log(attacker);

        Debug.Log($"{entityName} Taking {amount} damage");

        // 1. Process OnDamageTaken effects (they might modify the damage)
        // Iterate backwards if effects can remove themselves
        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            statusEffects[i].OnDamageTaken(attacker, ref amount, combatHud);
        }

        // 2. Apply actual damage
        if (amount > 0) // Don't "heal" from negative damage unless intended
        {
            SubtractHealth(amount);
        }

        // 3. Update HUD (or CombatManager does this after all effects)
        // UpdateHud(); // Let CombatManager handle HUD updates after a sequence of events

        // 4. Check for death (CombatManager might do this)
        if (!IsAlive())
        {
            // combatManagerInstance.HandleDeath(this);
        }
    }

    public string GetName() { return entityName; }
    public int GetHealth() { return health; }
    public int GetMaxHealth() { return maxHealth; }
    public virtual int GetStrength() { return strength; }
    public virtual int GetDexterity() { return dexterity; }
    public virtual int GetIntelligence() { return intelligence; }
    public bool IsAlive() {  return isAlive; }   

    public void SetStrength(int x) { strength = x; }
    public void SetDexterity(int x) { dexterity = x; }
    public void SetIntelligence (int x) { intelligence = x; }

    // For managing status effects
    public void AddStatusEffect(StatusEffect effectTemplate)
    {

        if (effectTemplate == null)
        {
            Debug.LogWarning("Effect is null");
            return;
        }

        // Create a runtime instance of the status effect
        StatusEffect effect = Instantiate(effectTemplate);
        statusEffects.Add(effect);
        effect.OnApply(this, combatManagerInstance, combatHud);
    }
    void RemoveStatusEffect(StatusEffect effect)
    {
        statusEffects.Remove(effect);
    }
    public List<StatusEffect> GetActiveStatusEffects()
    {
        return new List<StatusEffect>(statusEffects); // Return a copy
    }

    // To trigger HUD updates after effects
    public void UpdateHud()
    {
        //do something later
        return;
    }


    // TURN START
    public void ProcessTurnStartEffects()
    {
        // Iterate backwards if effects can remove themselves during their trigger
        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            statusEffects[i].OnTurnStart(combatHud);
        }
        // Remove expired effects after all have triggered to avoid modifying collection during iteration issues
        // (unless OnTurnStart itself can lead to removal, then careful iteration is needed)
        CleanupExpiredEffects();
    }

    // TURN END
    public void ProcessTurnEndEffects()
    {
        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            statusEffects[i].OnTurnEnd(combatHud);
            if (statusEffects[i].TickDown()) // Tick down duration
            {
                // Effect expired, OnRemove will be called by Cleanup
            }
        }
        CleanupExpiredEffects();
    }

    private void CleanupExpiredEffects()
    {
        List<StatusEffect> expiredEffects = new List<StatusEffect>();
        foreach (var effect in statusEffects)
        {
            if (effect.DurationTurns == 0) // Check if TickDown marked it for removal
            {
                expiredEffects.Add(effect);
            }
        }

        foreach (var expiredEffect in expiredEffects)
        {
            combatHud.QueueCombatMessage($"{GetName()}'s {expiredEffect.EffectName} wore off.");
            expiredEffect.OnRemove();
            RemoveStatusEffect(expiredEffect); // This will call OnRemove
        }
    }
    public virtual void AddAbility(AbilityBase ability)
    {
        if (ability != null && !Abilities.Contains(ability))
        {
            Abilities.Add(ability);
        }
    }

    public virtual void RemoveAbility(AbilityBase ability)
    {
        Abilities.Remove(ability);
    }
}
