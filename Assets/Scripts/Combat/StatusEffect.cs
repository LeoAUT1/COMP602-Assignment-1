public abstract class StatusEffect
{
    public string EffectName { get; protected set; }
    public string Description { get; protected set; }
    public int DurationTurns { get; protected set; } // -1 for permanent
    public bool IsHarmful { get; protected set; } // For UI or AI logic

    protected CombatEntity Target;
    protected CombatManager CombatManagerRef; // To queue messages or access combat state
    protected CombatHud CombatHudRef;

    public StatusEffect(string name, string description, int duration, bool isHarmful = false)
    {
        EffectName = name;
        Description = description;
        DurationTurns = duration;
        IsHarmful = isHarmful;
    }

    // Called when the effect is first applied to the target
    public virtual void OnApply(CombatEntity target, CombatManager combatManager, CombatHud hud)
    {
        this.Target = target;
        this.CombatManagerRef = combatManager;
        this.CombatHudRef = hud;
        // combatManager.QueueCombatMessage($"{target.GetUnitName()} gains {EffectName}!");
    }

    // Called at the start of the target's turn
    public virtual void OnTurnStart(CombatHud hud) { }

    // Called at the end of the target's turn
    public virtual void OnTurnEnd(CombatHud hud) { }

    // Called when the target takes damage
    // 'source' is who dealt the damage, 'damageAmount' is pre-mitigation if you have that
    public virtual void OnDamageTaken(CombatEntity source, ref int damageAmount, CombatHud hud) { }

    // Called when the target deals damage
    public virtual void OnDamageDealt(CombatEntity victim, ref int damageAmount, CombatHud hud) { }

    // Called to decrement duration and check for expiration
    // Returns true if the effect expired and should be removed
    public virtual bool TickDown()
    {
        if (DurationTurns > 0)
        {
            DurationTurns--;
            if (DurationTurns == 0)
            {
                // CombatManagerRef.QueueCombatMessage($"{Target.GetUnitName()}'s {EffectName} wore off.");
                return true; // Expired
            }
        }
        return false; // Not expired or indefinite
    }

    // Called when the effect is removed (either by duration or dispel)
    public virtual void OnRemove()
    {
        // Cleanup logic if any
    }
}
