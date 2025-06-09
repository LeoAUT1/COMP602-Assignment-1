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
    [SerializeField] protected int level;
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

    public virtual void AutoScaleToPlayer()
    {
	    int playerLevel = Player.Instance.GetLevel();
	    int scale = (playerLevel - 1) * 10;

        health += scale;
        maxHealth += scale;
	    strength += scale;
	    dexterity += scale;
	    intelligence += scale;

	    level = playerLevel;
    }

    public void TakeDamage(int amount, CombatEntity attacker = null)
    {
        Debug.Log(attacker);

        Debug.Log($"{entityName} Taking {amount} damage");

        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            statusEffects[i].OnDamageTaken(attacker, ref amount, combatHud);
        }

        if (amount > 0)
        {
            SubtractHealth(amount);
        }


        if (!IsAlive())
        {

        }
    }

    public string GetName() { return entityName; }
    public int GetHealth() { return health; }
    public int GetMaxHealth() { return maxHealth; }
    public virtual int GetStrength() { return strength; }
    public virtual int GetDexterity() { return dexterity; }
    public virtual int GetIntelligence() { return intelligence; }
    public int GetLevel() { return level; }
    public bool IsAlive() {  return isAlive; }
    public void SetAlive(bool b) { isAlive = b; }

    public void SetStrength(int x) { strength = x; }
    public void SetDexterity(int x) { dexterity = x; }
    public void SetIntelligence (int x) { intelligence = x; }
    public void SetMaxHealth (int x) { maxHealth = x; }

    // For managing status effects
    public void AddStatusEffect(StatusEffect effectTemplate)
    {

        if (effectTemplate == null)
        {
            Debug.LogWarning($"Effect {effectTemplate}is null");
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

    public void ClearAllStatusEffects()
    {
        statusEffects = new List<StatusEffect>();
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

    public virtual void ResetAllAbilities()
    {
        Abilities = new List<AbilityBase>();
        Abilities.Add(new BasicAttackAbility());
    }

    //healing
    public void Heal(int amount)
    {
     health = Mathf.Min(health + amount, maxHealth);
    }

    //dodge attack based on dex
    public virtual float DodgeChance()
    {
        float dodgeChance = 0.0f + (dexterity * 0.01f);
        return Mathf.Clamp(dodgeChance,0f,0.9f); //max 90%
    }

    public virtual bool DodgeAttempt()
    {
        return Random.value < DodgeChance();
    }

    //crit chance based on int
    public virtual float CritChance()
    {
        float critChance = 0.0f + (intelligence * 0.01f);
        return Mathf.Clamp(critChance, 0f, 0.5f); //max 50%
    }

    public virtual bool CritAttempt()
    {
        return Random.value < CritChance();
    }
}
